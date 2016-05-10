# UmbracoInstantArticles
Facebook Instant Articles support for Umbraco CMS

# Overview
Facebook’s Instant Articles are a new way to create fast-loading articles within the Facebook app.  The plugin supports two methods of publishing content:

* The latest content is shared via an RSS feed that Facebook checks every few minutes: https://fee.org/instantfeed/
* Whenever content is published, the article is pushed to the Facebook Instant Article API.
* 
For both methods, the body of the article is formatted using the special Instant Articles Format.   One tricky aspect of publishing instant articles is that they require strict HTML5 syntax, which means no inline images or embeds.  They also don’t use our sites stylesheet, so proprietary styles have to be replaced with native HTML5 tags.  To render these elements, I use regular expressions and the Html Agility Pack to reformat the HTML.

If you want to adapt my code,  you will need to update it to refer the relevant Umbraco properties, add the page id and correct access token to web.config, and update the analytics section in the html template.
