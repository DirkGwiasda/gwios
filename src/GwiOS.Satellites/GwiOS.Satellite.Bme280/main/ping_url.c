#include "ping_url.h"

#include <stdio.h>

int ping_url_write(char *buffer, size_t buffer_size, const char *gwios_url, const char *name)
{
    const int length = snprintf(buffer, buffer_size, "%s/api/satellites/%s/ping", gwios_url, name);

    if (length < 0 || (size_t)length >= buffer_size)
    {
        return -1;
    }

    return length;
}
