#include "bearer_token.h"

#include <stddef.h>
#include <string.h>

#define SCHEME "bearer "

static char to_lowercase(char character)
{
    return character >= 'A' && character <= 'Z' ? (char)(character - 'A' + 'a') : character;
}

static bool starts_with_scheme(const char *authorization)
{
    for (size_t index = 0; SCHEME[index] != '\0'; index++)
    {
        if (to_lowercase(authorization[index]) != SCHEME[index])
        {
            return false;
        }
    }

    return true;
}

static bool equals_in_constant_time(const char *candidate, const char *expected, size_t length)
{
    unsigned char difference = 0;
    for (size_t index = 0; index < length; index++)
    {
        difference |= (unsigned char)(candidate[index] ^ expected[index]);
    }

    return difference == 0;
}

bool bearer_token_matches(const char *authorization, const char *expected_token)
{
    if (authorization == NULL || !starts_with_scheme(authorization))
    {
        return false;
    }

    const char *candidate = authorization + strlen(SCHEME);
    const size_t length = strlen(expected_token);

    // Only the length can leak through timing, and every valid token has one of few possible lengths.
    if (strlen(candidate) != length)
    {
        return false;
    }

    return equals_in_constant_time(candidate, expected_token, length);
}
