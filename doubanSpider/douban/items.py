# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# http://doc.scrapy.org/en/latest/topics/items.html

import scrapy


class DoubanItem(scrapy.Item):
    # define the fields for your item here like:
    # name = scrapy.Field()
    id = scrapy.Field()
    name = scrapy.Field()
    director = scrapy.Field()
    actor = scrapy.Field()
    classification = scrapy.Field()
    score = scrapy.Field()
    year = scrapy.Field()
    abstracts = scrapy.Field()
    image_urls = scrapy.Field()
    images = scrapy.Field()
