#include <stddef.h>

#include "bearer_token.h"
#include "unity.h"

#define TOKEN "Kq3vX9_pL2-mN8rT5wY1zA4bC7dE0fGh"

void setUp(void)
{
}

void tearDown(void)
{
}

static void test_matches_the_expected_token(void)
{
    TEST_ASSERT_TRUE(bearer_token_matches("Bearer " TOKEN, TOKEN));
}

static void test_matches_the_scheme_in_any_letter_case(void)
{
    TEST_ASSERT_TRUE(bearer_token_matches("bearer " TOKEN, TOKEN));
    TEST_ASSERT_TRUE(bearer_token_matches("BEARER " TOKEN, TOKEN));
}

static void test_rejects_a_missing_header(void)
{
    TEST_ASSERT_FALSE(bearer_token_matches(NULL, TOKEN));
}

static void test_rejects_an_empty_header(void)
{
    TEST_ASSERT_FALSE(bearer_token_matches("", TOKEN));
}

static void test_rejects_a_token_that_differs_in_one_character(void)
{
    TEST_ASSERT_FALSE(bearer_token_matches("Bearer Kq3vX9_pL2-mN8rT5wY1zA4bC7dE0fGi", TOKEN));
}

static void test_rejects_a_token_in_another_letter_case(void)
{
    TEST_ASSERT_FALSE(bearer_token_matches("Bearer kq3vx9_pl2-mn8rt5wy1za4bc7de0fgh", TOKEN));
}

static void test_rejects_a_prefix_or_extension_of_the_token(void)
{
    TEST_ASSERT_FALSE(bearer_token_matches("Bearer Kq3vX9_pL2-mN8rT5wY1zA4bC7dE0fG", TOKEN));
    TEST_ASSERT_FALSE(bearer_token_matches("Bearer " TOKEN "x", TOKEN));
}

static void test_rejects_another_scheme(void)
{
    TEST_ASSERT_FALSE(bearer_token_matches("Basic " TOKEN, TOKEN));
    TEST_ASSERT_FALSE(bearer_token_matches(TOKEN, TOKEN));
}

static void test_rejects_the_scheme_without_separating_space(void)
{
    TEST_ASSERT_FALSE(bearer_token_matches("Bearer" TOKEN, TOKEN));
}

static void test_rejects_a_header_shorter_than_the_scheme(void)
{
    TEST_ASSERT_FALSE(bearer_token_matches("Bear", TOKEN));
}

int main(void)
{
    UNITY_BEGIN();
    RUN_TEST(test_matches_the_expected_token);
    RUN_TEST(test_matches_the_scheme_in_any_letter_case);
    RUN_TEST(test_rejects_a_missing_header);
    RUN_TEST(test_rejects_an_empty_header);
    RUN_TEST(test_rejects_a_token_that_differs_in_one_character);
    RUN_TEST(test_rejects_a_token_in_another_letter_case);
    RUN_TEST(test_rejects_a_prefix_or_extension_of_the_token);
    RUN_TEST(test_rejects_another_scheme);
    RUN_TEST(test_rejects_the_scheme_without_separating_space);
    RUN_TEST(test_rejects_a_header_shorter_than_the_scheme);
    return UNITY_END();
}
