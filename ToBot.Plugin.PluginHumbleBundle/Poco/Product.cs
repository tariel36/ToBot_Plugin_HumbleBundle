// The MIT License (MIT)
//
// Copyright (c) 2022 tariel36
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToBot.Plugin.PluginHumbleBundle.Poco
{
    public class Product
    {
        public TileLogoInformation tile_logo_information { get; set; }
        public string machine_name { get; set; }
        public string high_res_tile_image { get; set; }
        public bool disable_hero_tile { get; set; }
        public string marketing_blurb { get; set; }
        public string hover_title { get; set; }
        public string product_url { get; set; }
        public string tile_image { get; set; }
        public HighResTileImageInformation high_res_tile_image_information { get; set; }
        public List<HeroHighlight> hero_highlights { get; set; }
        public List<string> hover_highlights { get; set; }
        public string fallback_store_sale_logo { get; set; }
        public bool supports_partners { get; set; }
        public string detailed_marketing_blurb { get; set; }
        public string tile_logo { get; set; }
        public string tile_short_name { get; set; }

        [JsonProperty("start_date|datetime")]
        public DateTime StartDateDatetime { get; set; }

        [JsonProperty("end_date|datetime")]
        public DateTime EndDateDatetime { get; set; }
        public string tile_stamp { get; set; }

        [JsonProperty("bundles_sold|decimal")]
        public double BundlesSoldDecimal { get; set; }
        public string tile_name { get; set; }
        public string short_marketing_blurb { get; set; }
        public TileImageInformation tile_image_information { get; set; }
        public string type { get; set; }
        public List<string> highlights { get; set; }
    }
}