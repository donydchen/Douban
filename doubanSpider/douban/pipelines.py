# -*- coding: utf-8 -*-

# Define your item pipelines here
#
# Don't forget to add your pipeline to the ITEM_PIPELINES setting
# See: http://doc.scrapy.org/en/latest/topics/item-pipeline.html

import scrapy
import logging
import sqlite3 as sqlite
from scrapy.pipelines.images import ImagesPipeline
import re
from scrapy.http import Request


class DoubanPipeline(object):

    def __init__(self):
        self.connection = sqlite.connect('doubanmovie.sqlite')
        self.cursor = self.connection.cursor()
        self.cursor.execute('CREATE TABLE IF NOT EXISTS Top250 ' \
            '(id INTEGER PRIMARY KEY, name VARCHAR, director VARCHAR, actor VARCHAR, classification VARCHAR, score REAL, year INTEGER, abstracts VARCHAR)')

    def process_item(self, item, spider):
        movie_id = int(item['id'])
        self.cursor.execute("SELECT * FROM Top250 WHERE id=%d" % movie_id)
        if self.cursor.fetchone():
            logging.log(logging.DEBUG, "Movie already in table: %d" % movie_id)
        else:
            # escape the single quote of a string, otherwise it couldn't be inserted 
            movie_name = str(item['name']).replace("'", "''")
            movie_abstracts = str(item['abstracts']).replace("'", "''")
            self.cursor.execute(
                "INSERT INTO Top250(id, name, director, actor, classification, score, year, abstracts) VALUES (%d, '%s', '%s', '%s', '%s', %.1f, %d, '%s')" %
                (movie_id, movie_name, str(item['director']), str(item['actor']), str(item['classification']), float(item['score']), int(item['year']), movie_abstracts)
            )
            self.connection.commit()
            logging.log(logging.DEBUG, 'Movie stored: %d' % movie_id)

        return item

    def handle_error(self, e):
        log.err(e)


class DoubanImagePipeline(ImagesPipeline):

    def get_media_requests(self, item, info):
        logging.log(logging.DEBUG, "Get Media Request successfully!")
        for image_url in item['image_urls']:
            yield scrapy.Request(image_url, meta={"image_names": [item["id"]]})

    def get_images(self, response, request, info):
        for key, image, buf in super(DoubanImagePipeline, self).get_images(response, request, info):
            if re.compile('^full/[0-9,a-f]+.jpg$').match(key):
                logging.log(logging.DEBUG, "Image name matched successfully.")
                key = self.change_filename(response)
            yield key, image, buf

    def change_filename(self, response):
        return "%s.jpg" % response.meta['image_names'][0]
