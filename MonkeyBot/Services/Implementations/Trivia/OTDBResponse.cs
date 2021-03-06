﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MonkeyBot.Services
{
    //https://opentdb.com/api_config.php

    public class OTDBResponse
    {
        [JsonPropertyName("response_code")]
        public TriviaApiResponse Response { get; set; }

        [JsonPropertyName("results")]
        public List<OTDBQuestion> Questions { get; set; }
    }

    public enum TriviaApiResponse
    {
        Success = 0,
        NoResults = 1,
        InvalidParameter = 2,
        TokenNotFound = 3,
        TokenEmpty = 4
    }
}