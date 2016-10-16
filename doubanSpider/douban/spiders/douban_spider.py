# _*_ coding: utf-8 _*_

import scrapy
from douban.items import DoubanItem
from w3lib.html import remove_tags
from scrapy.xlib.pydispatch import dispatcher
from scrapy import signals


class DoubanSpider(scrapy.Spider):
    name = "douban"
    start_urls = [
        'https://movie.douban.com/top250'
    ]
    handle_httpstatus_list = [404, 500]

    def __init__(self, category=None):
        # Use to record the fail download url
        self.failed_urls = {}

    def parse(self, response):
        for item_url in response.css('div.item div.hd a::attr(href)').extract():
            yield scrapy.Request(response.urljoin(item_url.encode('utf-8')), callback=self.parse_movie)

        next_page = response.css('.next > a:nth-child(2)::attr(href)').extract_first()
        if next_page is not None:
            next_page = response.urljoin(next_page)
            yield scrapy.Request(next_page, callback=self.parse)

    def parse_movie(self, response):

        def extract_with_css(addr):
            return response.css(addr).extract_first().strip().encode('utf-8')

        if response.status in self.handle_httpstatus_list:
            self.crawler.stats.inc_value('failed_url_count')
            status_str = str(response.status)
            if not status_str in self.failed_urls:
                self.failed_urls[status_str] = []
            self.failed_urls[status_str].append(response.url)
        else:
            movie_item = DoubanItem()
            movie_item['id'] = response.css('.top250-no::text').extract_first().split('.')[-1].encode('utf-8')
            movie_item['name'] = extract_with_css('#content > h1:nth-child(2) > span:nth-child(1)::text')
            movie_item['director'] = extract_with_css('#info > span:nth-child(1) > span:nth-child(2) > a:nth-child(1)::text')
            # use only the first five actors
            actor_list = response.css('.actor span.attrs a::text').extract()
            movie_item['actor'] = '/'.join(actor_list[:5]).encode('utf-8')
            # get the class list of the movie
            class_list = []
            for span_item in response.css('#info span'):
                if span_item.css('::attr(property)').extract_first() == 'v:genre':
                    class_list.append(span_item.css('::text').extract_first())
            movie_item['classification'] = '/'.join(class_list).encode('utf-8')
            # get score and year
            movie_item['score'] = extract_with_css('strong.ll::text')
            movie_item['year'] = response.css('.year').re(r'[0-9]\d*')[0].encode('utf-8')
            # get whole abstract and set up the display format
            abstract_html = response.css('#link-report span')[-2].extract()
            html_list = []
            for tokens in remove_tags(abstract_html).split('\n'):
                tokens = tokens.strip()
                if tokens:
                    html_list.append(tokens)
            movie_item['abstracts'] = (u'\u3000\u3000' + u'\n\u3000\u3000'.join(html_list)).encode('utf-8')

            # get movie poster
            movie_item['image_urls'] = response.css('.nbgnbg > img:nth-child(1)::attr(src)').extract()

            yield movie_item

    def process_exception(self, response, exception, spider):
        """
        To track Twisted errors, it will only appear if exceptions are actually thrown
        """
        ex_class = "%s.%s" % (exception.__class__.__module__, exception.__class__.__name__)
        self.crawler.stats.inc_value('downloader/exception_count', spider=spider)
        self.crawler.stats.inc_value('downloader/exception_type_count/%s' % ex_class, spider=spider)

    def handle_spider_closed(spider, reason):
        """
        print out the failed_urls when the spider close, and also log it to file failed_rsps.log
        """
        if len(spider.failed_urls) > 0:
            kv_urls = []
            for k, v in spider.failed_urls.iteritems():
                kv_url = k + ': ' + ', '.join(v)
                kv_urls.append(kv_url)
            spider.crawler.stats.set_value('failed_urls', '; '.join(kv_urls))
            with open('failed_rsps.log', 'wb') as f:
                f.write('\n'.join(kv_urls))

    # register the signal
    dispatcher.connect(handle_spider_closed, signals.spider_closed)
