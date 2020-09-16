# SiteScraper
A simple bit of code to try and scrape a website and store it locally.

## Status
This is very early days, but it is at least capturing internal HTML and IMG files and following relative links to discover other pages. 

It will need more than this and a lot more thought, but as an experimental bit of code, its in a good state to play with if you don't expect too much from it.

Hopefully I'll get the chance to improve the code and make more progress before anyone else grabs it, but right now, I just wanted to create this repo and post what I had to it, which is essentially what I put together in a couple of hours as a proof of concept.

I'm writing this for one very specific project, but would like to think I can generalise it.

## Technology
This uses .NET Core for experimentation, and [AngleSharp](https://github.com/AngleSharp/AngleSharp) for convenience. Both of these decisions may change later.

The code is written with Visual Studio 2019 Community edition, but that shouldn't be a barrier to anyone else picking up this code. 