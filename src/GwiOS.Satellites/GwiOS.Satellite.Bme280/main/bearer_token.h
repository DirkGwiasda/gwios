#pragma once

#include <stdbool.h>

#ifdef __cplusplus
extern "C" {
#endif

// Tells whether the value of an Authorization header carries the expected token as "Bearer <token>". The
// scheme is matched case-insensitively as HTTP requires. The token itself is compared in constant time, so
// the response time does not reveal how many leading characters of a guess were right. A missing header
// (NULL) never matches.
bool bearer_token_matches(const char *authorization, const char *expected_token);

#ifdef __cplusplus
}
#endif
