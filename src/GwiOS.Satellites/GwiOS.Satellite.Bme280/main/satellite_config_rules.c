#include "satellite_config_rules.h"

#include <string.h>

#define HTTPS_PREFIX "https://"

// Checks characters one by one instead of using ctype.h: the rules are about ASCII only, and ctype
// functions depend on the locale and are undefined for negative char values.
static bool is_lowercase_letter(char character)
{
    return character >= 'a' && character <= 'z';
}

static bool is_uppercase_letter(char character)
{
    return character >= 'A' && character <= 'Z';
}

static bool is_digit(char character)
{
    return character >= '0' && character <= '9';
}

static bool is_printable_ascii(char character)
{
    return character >= ' ' && character <= '~';
}

static bool is_name_character(char character)
{
    return is_lowercase_letter(character) || is_digit(character) || character == '-';
}

static bool is_token_character(char character)
{
    return is_lowercase_letter(character) || is_uppercase_letter(character) || is_digit(character) || character == '-'
           || character == '_';
}

static bool is_url_character(char character)
{
    return is_printable_ascii(character) && character != ' ';
}

static bool consists_of(const char *value, bool (*is_allowed)(char))
{
    for (const char *character = value; *character != '\0'; character++)
    {
        if (!is_allowed(*character))
        {
            return false;
        }
    }

    return true;
}

static bool has_length_between(const char *value, size_t min_length, size_t max_length)
{
    const size_t length = strlen(value);

    return length >= min_length && length <= max_length;
}

bool satellite_config_is_valid_name(const char *name)
{
    return has_length_between(name, 1, SATELLITE_NAME_MAX_LENGTH) && consists_of(name, is_name_character);
}

bool satellite_config_is_valid_token(const char *token)
{
    return has_length_between(token, SATELLITE_TOKEN_MIN_LENGTH, SATELLITE_TOKEN_MAX_LENGTH)
           && consists_of(token, is_token_character);
}

bool satellite_config_is_valid_wifi_ssid(const char *ssid)
{
    return has_length_between(ssid, 1, SATELLITE_WIFI_SSID_MAX_LENGTH);
}

bool satellite_config_is_valid_wifi_password(const char *password)
{
    if (password[0] == '\0')
    {
        return true;
    }

    return has_length_between(password, 8, SATELLITE_WIFI_PASSWORD_MAX_LENGTH) && consists_of(password, is_printable_ascii);
}

bool satellite_config_is_valid_gwios_url(const char *url)
{
    const size_t prefix_length = strlen(HTTPS_PREFIX);
    const size_t length = strlen(url);

    if (length <= prefix_length || length > SATELLITE_GWIOS_URL_MAX_LENGTH)
    {
        return false;
    }

    if (strncmp(url, HTTPS_PREFIX, prefix_length) != 0)
    {
        return false;
    }

    // A trailing slash would produce "//api/..." once the endpoint path is appended.
    if (url[length - 1] == '/')
    {
        return false;
    }

    return consists_of(url, is_url_character);
}
