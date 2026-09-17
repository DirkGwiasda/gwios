#pragma once

#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

// Writes the URL of the ping endpoint, "<gwios_url>/api/satellites/<name>/ping". Both parts must already
// follow the configuration rules, so neither needs encoding. Returns the length written without the
// terminating zero, or -1 if the buffer is too small, in which case its content must not be used.
int ping_url_write(char *buffer, size_t buffer_size, const char *gwios_url, const char *name);

#ifdef __cplusplus
}
#endif
