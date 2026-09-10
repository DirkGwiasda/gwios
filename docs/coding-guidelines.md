# Coding Guidelines

Binding for all code written in this repository. 

Rules are numbered so they can be cited in reviews and amended individually. New rules are
added only on explicit instruction (R3).

Sources: Robert C. Martin, *Clean Code* (principles restated in our own words) and Microsoft
Learn — [C# Coding Conventions][ms-conv], [Capitalization Conventions][ms-caps],
[Framework Design Guidelines][ms-fdg].

---

## CORE — Core rules

These rules outrank every other rule in this document. Where another rule appears to conflict
with them, these win.

- **CORE-1 — Single Responsibility Principle.** The most important principle in this project.
  It applies at **both** levels:
  - **Class level:** a type has exactly one reason to change. Mixing protocol handling with
    process management, or rendering with input parsing, is a violation.
  - **Method level:** a method does exactly one thing, at one level of abstraction. If a
    method needs a comment to separate its "phases", those phases are separate methods.

  When in doubt, split. A type or method that is hard to name precisely is doing too much.

- **CORE-2 — One type per file, always.** Every `class`, `record`, `interface`, `enum`,
  `struct`, and `delegate` lives in its own file. No exceptions, regardless of how small the
  type is.
  - The file name matches the type name exactly (`AgentTextEvent` → `AgentTextEvent.cs`).
  - **Nested types** are part of their containing type and stay in that type's file — C#
    permits no other placement. Nesting is therefore a deliberate design statement: the inner
    type is meaningless outside the outer one.

- **CORE-3 — The namespace mirrors the folder structure.** A type's namespace is the project's
  root namespace followed by the folder path from the project file down to the type's own file.
  Where **CORE-2** binds the file name to the type name, this binds the folder to the namespace:
  together they make the place a type lives in derivable from its full name, and the reverse.
  - Only folders *below* the project file count. `GwiOS.Core/ToDos/Domain/Workflow/Contracts/`
    inside the project `GwiOS.Core` gives
    `GwiOS.Core.ToDos.Domain.Workflow.Contracts` — the `src` folder above the project file
    contributes nothing.
  - Moving a file into another folder therefore always means changing its namespace, and every
    `using` that pointed at it. That cost is intended: it keeps the two from drifting apart.
  - This is what the compiler's IDE0130 checks, so the rule is enforceable rather than a matter
    of review discipline.

---

## DI — Dependency Injection

Architectural ground rules. They are outranked only by **CORE**, and they sharpen **CC-20**:
depending on abstractions is not a judgement call here, it is the default.

- **DI-1 — Every type with behaviour has its own interface.** A type that does something —
  orchestrating, calling, reading, writing, deciding, rendering — is declared as `IFoo` and
  implemented as `Foo`. Interface and implementation each live in their own file (**CORE-2**).

- **DI-2 — Collaborators are received through constructor injection of the interface.**
  Never `new` another service inside a service, never reach for a service locator, never
  access a service through a static member. A type's constructor lists its dependencies as
  interfaces; that list is the type's contract with the container.

- **DI-3 — Keep implementations closed.** Concrete types are `internal` and `sealed` wherever
  the design allows it. The interface is the public surface; the class is a detail the
  composition root happens to know about.

- **DI-4 — No static classes, no extension methods, no static mutable state.** Behaviour lives
  in instance members of an injectable type. Two static constructs are permitted:
  - The program entry point, which the language itself requires.
  - **Container registration extensions** — static `IServiceCollection` / `IHostBuilder`
    extension methods such as `AddCoderBackend(…)`, which follow the established .NET hosting
    idiom. They belong to the composition root and may contain registration calls only: no
    domain logic, no I/O, no state.

- **DI-5 — Exempt from DI-1.** These types need no interface of their own:
  - **POCO / model types** — DTOs, records, domain events, value objects, options and
    configuration classes. Data without behaviour.
  - **Exception types.**
  - **Enums, structs, delegates, attributes.**
  - **The composition root** — `Program` and the registration code that wires the container.
    Knowing concrete types is precisely its job.
  - **Types that already satisfy a framework abstraction** — an `IHostedService`,
    an `IChatClient` implementation, a `JsonConverter<T>`. The abstraction exists; do not add
    a second one on top.
  - **Abstract base classes and template-method bases** — the base class *is* the abstraction.
    Consumers still inject the interface of the concrete derived type, never the base.
  - **Private and nested helper types** that never leave their containing type. They are
    implementation detail, not collaborators.
  - **Test code** — test classes, fixtures, fakes and stubs. A fake implements the production
    interface; it does not need one of its own.

  Anything touching the clock, file system, environment, processes or the network is **never**
  exempt. That is exactly what makes tests unreliable (**CC-30**), and it belongs behind an
  interface.

- **DI-6 — An injected collaborator is captured in a `private readonly` field.** Where a primary
  constructor is used, its parameter is *not* the storage for the dependency: it is named in
  camelCase (**MS-2**) and assigned once to a `_camelCase` field (**MS-4**), which is what the
  members use.

  ```csharp
  internal sealed class CodingAgentContextManager(ICodingAgentContextRepository codingAgentContextRepository)
      : ICodingAgentContextManager
  {
      private readonly ICodingAgentContextRepository _codingAgentContextRepository = codingAgentContextRepository;
  }
  ```

  This keeps both naming rules intact at the same time, and it buys immutability the language
  cannot otherwise express: a primary constructor parameter is not `readonly` and can be
  reassigned from any member. A dependency must not be reassignable.

---

## DOC — Contract documentation

Contracts are the public surface of this codebase (**DI-1**, **DI-3**): they are what a caller
reads before writing a single line against a type. An undocumented contract therefore costs every
future reader the same investigation. Two things count as contract here — interfaces wherever they
live (**DOC-1**), and everything inside a `Contracts` folder (**DOC-2**). These rules are
mandatory, not a matter of taste, and they sharpen **MS-42**.

- **DOC-1 — Every interface and every one of its members carries an XML `<summary>`.** This
  covers the interface declaration itself and *all* of its members without exception — methods,
  properties, events and indexers alike — regardless of how self-explanatory the name looks.
  A summary states what the member is responsible for, not how an implementation does it.

  ```csharp
  /// <summary>
  /// Persists and retrieves the conversation context of a coding agent.
  /// </summary>
  internal interface ICodingAgentContextRepository
  {
      /// <summary>
      /// Returns the stored context for the given agent, or an empty context if none exists yet.
      /// </summary>
      Task<CodingAgentContext> GetAsync(string agentId, CancellationToken cancellationToken);
  }
  ```

- **DOC-2 — Everything below a `Contracts` folder is documented, whatever its kind.** A type placed
  in such a folder is declared to be part of the contract, so the documentation duty follows from
  its *location*, not from its kind: records, classes, enums, structs and delegates are held to the
  same standard as the interfaces next to them. Both the type declaration and every member it
  exposes — properties, methods, events, enum values — carry a `<summary>`.
  - This includes every subfolder, however deep: `Contracts/Models/AgentContext/AgentMessage.cs`
    is covered exactly like `Contracts/IAgentWorkflow.cs`.
  - Models are the common trap. "Data without behaviour" exempts a type from needing an interface
    (**DI-5**), never from being documented. A property whose meaning is guessable from its name
    still needs its constraints and units spelled out — what a `Timeout` is measured in, whether a
    collection may be empty, what an unset value means.

- **DOC-3 — XML documentation is always written in English.** Identifiers are English, and so is
  every `<summary>` and every other XML documentation tag — regardless of the language used for
  `//` comments elsewhere in the file.

- **DOC-4 — Missing summaries are fixed, never left in place.** When you come across a contract
  whose type declaration or members are undocumented, add the missing summaries as part of your
  change. This is the one documented extension of the Boy Scout Rule (**CC-22**) that is not
  optional.

- **DOC-5 — Ask instead of guessing.** If the responsibility of a type or of an individual member
  is not **one hundred percent** clear from the surrounding context, stop and ask the developer.
  A plausible-sounding but wrong summary is worse than none at all (**CC-17**) — it is read as fact
  and propagated into every implementation and call site.

---

## CC — Clean Code

### Names

- **CC-1** Names must reveal intent. If a name needs an explanatory comment, the name is wrong.
- **CC-2** Avoid disinformation: no misleading names, no `list` suffix for something that is not a list.
- **CC-3** Make names searchable and pronounceable. Single letters only as loop counters in a tiny scope.
- **CC-4** No type or scope encodings in names (no Hungarian notation, no `m_` prefixes).
- **CC-5** One word per concept: do not mix `Fetch`, `Retrieve` and `Get` for the same kind of operation.
- **CC-6** Class names are nouns, method names are verbs.

### Functions

- **CC-7** Functions are small and do exactly one thing → binding statement in **CORE-1**.
- **CC-8** All statements in a function sit at the same level of abstraction.
- **CC-9** Prefer few parameters (0–3). More than three is a sign that a parameter object is missing.
- **CC-10** No boolean flag parameters — a flag means the function does two things. Split it.
- **CC-11** No hidden side effects: a function does what its name says and nothing else.
- **CC-12** Command–query separation: a function either changes state or returns a value, not both.
- **CC-13** Extract nested conditionals and loops into named methods rather than deepening indentation.

### Comments

- **CC-14** Comments explain *why*, not *what*. Code explains what.
- **CC-15** Do not comment bad code — rewrite it.
- **CC-16** Never commit commented-out code. Version control keeps history.
- **CC-17** A wrong comment is worse than none: update comments together with the code they describe.

### Structure and design

- **CC-18** Single responsibility: a class has exactly one reason to change → binding statement in **CORE-1**.
- **CC-19** DRY — remove duplication, but do not couple unrelated code just because it looks similar.
- **CC-20** Depend on abstractions, not on concrete implementations. In this repository this is not conditional — see **DI-1** and **DI-2**.
- **CC-21** Keep related code vertically close; declare variables near their first use.
- **CC-22** Boy Scout Rule: leave touched code cleaner than you found it — without turning a change into an unrelated refactoring.

### Error handling

- **CC-23** Use exceptions, not error codes or magic return values.
- **CC-24** Provide context in exception messages: what failed, and with which input.
- **CC-25** Do not return `null` and do not pass `null` where an object is expected. Prefer empty collections, `Try…` patterns, or nullable reference types made explicit.
- **CC-26** Error handling is one thing: a method that handles errors should do only that.

### Tests

- **CC-27** Test code is production code and follows the same standards.
- **CC-28** One concept per test; a test name states the scenario and the expected outcome.
- **CC-29** F.I.R.S.T. — fast, independent, repeatable, self-validating, written timely.
- **CC-30** Tests must not depend on external services or on machine state. Use fakes at the boundary.

---

## MS — Microsoft C# conventions

### Naming and capitalization

- **MS-1** PascalCase for namespaces, types, interfaces, methods, properties, events, fields, enum values, and constants.
- **MS-2** camelCase for parameters and local variables.
- **MS-3** Interfaces start with `I`. Type parameters start with `T`.
- **MS-4** No underscores inside identifiers. Exception: the established `_camelCase` for private instance fields, as used throughout this repository.
- **MS-5** Acronyms of three letters or more are treated as words (`HtmlTag`, `Json`); two-letter acronyms keep both letters capitalized (`IOStream`), and lowercase both when they start a camelCased name (`ioStream`).
- **MS-6** Treat closed-form compound words as single words: `Endpoint`, `FileName`, `Metadata`, `Username` → `UserName`, `Id` (not `ID`).
- **MS-7** Names must not differ by case alone.

### Language usage

- **MS-8** Use language keywords for types, not runtime types: `string` not `String`, `int` not `Int32`.
- **MS-9** Prefer `int` over unsigned types.
- **MS-10** Use modern language features; avoid outdated constructs.
- **MS-11** Catch only exceptions you can actually handle. Do not catch `System.Exception` without a filter.
- **MS-12** Throw specific exception types with meaningful messages.
- **MS-13** Use `async`/`await` for I/O-bound work; be mindful of deadlocks and use `ConfigureAwait` where appropriate.
- **MS-14** Use `&&` and `||`, not `&` and `|`, for comparisons — short-circuiting is usually required for correctness.
- **MS-15** Call static members through the type name, never through a derived type.
- **MS-16** Use file-scoped namespace declarations.
- **MS-17** Place `using` directives outside the namespace declaration.

### Implicit typing (`var`)

- **MS-18** Use `var` only when the type is obvious from the right-hand side — a `new` expression, an explicit cast, or a literal.
- **MS-19** Do not use `var` when the type is not apparent; a method name is not evidence of a type.
- **MS-20** Use explicit types for `foreach` loop variables; the element type of a collection is rarely obvious.
- **MS-21** Use `var` for LINQ query and range variables (anonymous and nested generic types).

### Strings, collections, delegates

- **MS-22** Use string interpolation for short concatenations; use the expression form, not positional placeholders.
- **MS-23** Use `StringBuilder` when appending in loops or building large amounts of text.
- **MS-24** Prefer raw string literals over escape sequences or verbatim strings.
- **MS-25** Use collection expressions (`string[] vowels = ["a", "e"];`) to initialize collections.
- **MS-26** Use `Func<>` and `Action<>` instead of declaring custom delegate types.
- **MS-27** Use object initializers and the concise `new` forms (`ExampleClass instance = new();`) when the variable type matches the object type.
- **MS-28** Use `required` properties rather than constructors to enforce initialization where it fits.

### Resources and exceptions

- **MS-29** Use `using` declarations instead of `try`/`finally` blocks whose only job is calling `Dispose`.
- **MS-30** Prefer the brace-less `using` declaration form.

### LINQ

- **MS-31** Give query variables meaningful names.
- **MS-32** Place `where` clauses before other clauses so later clauses work on the reduced set.
- **MS-33** Alias anonymous-type properties so their names are unambiguous and PascalCased.
- **MS-34** Access inner collections with multiple `from` clauses rather than `join`.

### Layout

- **MS-35** Four spaces per indent level, never tabs.
- **MS-36** Allman braces: opening and closing brace each on their own line, aligned with the current indent.
- **MS-37** One statement and one declaration per line.
- **MS-38** At least one blank line between method and property definitions.
- **MS-39** Break long statements across lines; line breaks go *before* binary operators.
- **MS-40** Use parentheses to make precedence explicit in compound conditions.

### Comments and XML documentation

- **MS-41** Use `//` for explanations; avoid `/* */` blocks.
- **MS-42** Use XML documentation comments for types and all public members. For interfaces and for
  everything below a `Contracts` folder this is not a recommendation but a hard requirement — see
  **DOC-1** and **DOC-2**.
  - **Exception: test methods.** A test method carries no `<summary>`. **CC-28** already requires its
    name to state the scenario and the expected outcome, and a summary would say the same thing a
    second time — which is the kind of duplication that goes stale first (**CC-17**). Everything else
    in a test project stays documented: the test class says what area it covers, and a fake says what
    it stands in for and how it behaves.
- **MS-43** Put comments on their own line, not trailing a line of code.
- **MS-44** Start comment text with a capital letter, end it with a period, and put one space after `//`.

### Asynchronous methods

- **MS-45** Methods that return `Task` or `Task<T>` carry the `Async` suffix, following the
  Task-based Asynchronous Pattern: `GetAllAsync`, `SaveAsync`, `ContinueAsync`. The suffix is part
  of the contract, so it is declared on the interface and matched by every implementation.

---

## Notes on deviations

- Microsoft's 65-character line limit targets documentation samples on mobile screens and is
  **not** adopted here; the effective limit in this repository is currently ~120 characters.

[ms-conv]: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
[ms-caps]: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions
[ms-fdg]: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
