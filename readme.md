# Simple Search Engine
A simple web-based search engine developed using C# ASP.Net
# Steps
## To run Crawler
- Create SQL Server Database containing files as in SearchEngine.cs
- Run Crawrler() function separetly to crawl URLs (bbc.com, if want another website change URL variable to desired URL)
- Run Indexer() function to save crawled URLs to database 
## To Run Search Engine
- Run WebInterface.csproj 
- Open browser and enter any text to search
