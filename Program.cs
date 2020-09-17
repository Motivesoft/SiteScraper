using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace SiteScraper
{
    class Program
    {
        private static IList<string> urls = new List<string>();

        static void Main( string[] args )
        {
            Console.WriteLine( "Hello World!" );
            ScrapeAsync( "http://www.motivesoft.demon.co.uk/cyberzoo/instruct.htm", "zzz" ).Wait();

            if ( args.Length != 2 )
            {
                Console.Error.WriteLine( "Arguments required: URL path" );
            }
            else
            {
                string url = args[ 0 ];
                string path = args[ 1 ];
                Directory.CreateDirectory( path );
                ScrapeAsync( url, path ).Wait();
            }
        }

        private static async System.Threading.Tasks.Task ScrapeAsync( string pageUrl, string outputLocation )
        {
            if ( urls.Contains( pageUrl ) )
            {
                return;
            }
            urls.Add( pageUrl );

            Console.WriteLine( $"Traversing link: {pageUrl}" );

            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage request = await httpClient.GetAsync( pageUrl );
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument( response );

            Url url = new Url( pageUrl );
            Console.WriteLine( "ContentLength: " + request.Content.Headers.ContentLength );

            string p = Path.Combine( outputLocation, url.Path );
            Console.WriteLine( "Writing to --> " + p );
            Directory.CreateDirectory( Path.GetDirectoryName( p ) );
            Console.WriteLine( document.ToHtml() );
            /*
            using ( Stream fs = File.OpenWrite( p  ) )
            {
                response.Seek( 0, SeekOrigin.Begin );
                response.CopyTo( fs );
                //await response.CopyToAsync( fs );
                //await fs.FlushAsync();
            }*/

            var els = document.All.Where( x => x.NodeType == NodeType.Element );
            foreach ( var e in els )
            {
                Console.WriteLine( $"{e.NodeName} {e.GetType()}" );
                if ( ( e as IHtmlElement ).HasAttribute( "background" ) )
                {
                    Url imgUrl = Url.Create( ( e as IHtmlElement ).GetAttribute( "background" ) );
                    if ( imgUrl.IsRelative )
                    {
                        Url hrefUrl = imgUrl;

                        string follow = hrefUrl.Href;
                        Url newUrl = new Url( url, follow );

                        CancellationTokenSource cancellationToken2 = new CancellationTokenSource();
                        HttpClient httpClient2 = new HttpClient();
                        HttpResponseMessage request2 = await httpClient2.GetAsync( newUrl );
                        cancellationToken2.Token.ThrowIfCancellationRequested();

                        Console.WriteLine( "Downloading: " + newUrl );

                        byte[] response2 = await request2.Content.ReadAsByteArrayAsync();
                        cancellationToken2.Token.ThrowIfCancellationRequested();

                        string p2 = Path.Combine( outputLocation, newUrl.Path );
                        Directory.CreateDirectory( Path.GetDirectoryName( p2 ) );
                        Console.WriteLine( "Writing to --> " + p2 );
                        using ( Stream fs = File.OpenWrite( p2 ) )
                        {
                            var l = request2.Content.Headers.ContentLength ?? response2.Length;
                            Console.WriteLine( $"***Writing {l} chars {response2.Length}" );
                            fs.Write( response2 );
                            fs.Flush();
                        }
                    }
                }
            }

            var imgs = document.All
                .Where( x => x.NodeType == NodeType.Element )
                .OfType<IHtmlImageElement>();
            if ( imgs == null )
            {
                Console.WriteLine( "No images in: " + pageUrl );
            }
            else
            {
                foreach ( var i in imgs )
                {
                    string src = i.Source;
                    if ( i.HasAttribute( "src" ) )
                    {
                        src = i.GetAttribute( "src" );
                    }
                    Url imgUrl = Url.Create( src );
                    if ( imgUrl.IsRelative )
                    {
                        {
                            Url hrefUrl = imgUrl;
                            {
                                string follow = hrefUrl.Href;
                                if ( !string.IsNullOrEmpty( hrefUrl.Fragment ) )
                                {
                                    follow = follow.Substring( 0, follow.IndexOf( hrefUrl.Fragment ) - 1 );
                                }

                                Url newUrl = new Url( url, follow );

                                CancellationTokenSource cancellationToken2 = new CancellationTokenSource();
                                HttpClient httpClient2 = new HttpClient();
                                HttpResponseMessage request2 = await httpClient2.GetAsync( newUrl );
                                cancellationToken2.Token.ThrowIfCancellationRequested();

                                Console.WriteLine( "Downloading: " + newUrl );

                                Stream response2 = await request2.Content.ReadAsStreamAsync();
                                cancellationToken2.Token.ThrowIfCancellationRequested();

                                string p2 = Path.Combine( outputLocation, newUrl.Path );
                                Directory.CreateDirectory( Path.GetDirectoryName( p2 ) );
                                Console.WriteLine( "Writing to --> " + p2 );
                                using ( Stream fs = File.OpenWrite( p2 ) )
                                {
                                    response.Seek( 0, SeekOrigin.Begin );
                                    await response2.CopyToAsync( fs );
                                    await fs.FlushAsync();
                                    //response2.CopyTo( fs );
                                    //fs.Flush();
                                }
                            }
                        }
                    }

                    // No need
                    /*if ( !src.Contains( "/" ) )
                    {
                        i.SetAttribute("src", "./" + i.GetAttribute("src") );
                    }*/
                }
            }

            {
                CancellationTokenSource cancellationToken2 = new CancellationTokenSource();
                HttpClient httpClient2 = new HttpClient();
                HttpResponseMessage request2 = await httpClient2.GetAsync( pageUrl );
                cancellationToken2.Token.ThrowIfCancellationRequested();

                Console.WriteLine( "Downloading: " + pageUrl );

                byte[] response2 = await request2.Content.ReadAsByteArrayAsync();
                cancellationToken2.Token.ThrowIfCancellationRequested();

                string p2 = Path.Combine( outputLocation, url.Path );
                Directory.CreateDirectory( Path.GetDirectoryName( p2 ) );
                Console.WriteLine( "Writing to --> " + p2 );
                using ( Stream fs = File.OpenWrite( p2 ) )
                {
                    var l = request2.Content.Headers.ContentLength ?? response2.Length;
                    Console.WriteLine( $"***Writing {l} chars {response2.Length}" );
                    fs.Write( response2 );
                    fs.Flush();
                }
            }

            /* Write modified HTML 
            using ( StreamWriter fs = new StreamWriter( File.OpenWrite( p ) ) )
            {
                fs.Flush();
                fs.Write( document.ToHtml() );
                fs.Flush();
                fs.Close();
            }*/

            var refs = document.All.Where( x => x.IsLink() );
            foreach ( var r in refs )
            {
                foreach ( var a in r.Attributes )
                {
                    if ( a.Name.Equals( "href", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        string href = a.Value;

                        if ( href.StartsWith( "#" ) )
                        {
                            Console.WriteLine( "Skipping anchor link: " + href );
                            break;
                        }

                        Url hrefUrl = Url.Create( href );
                        if ( hrefUrl.IsRelative )
                        {
                            string follow = hrefUrl.Href;
                            if ( !string.IsNullOrEmpty( hrefUrl.Fragment ) )
                            {
                                follow = follow.Substring( 0, follow.IndexOf( hrefUrl.Fragment ) - 1 );
                            }

                            Url newUrl = new Url( url, follow );
                            await ScrapeAsync( newUrl.Href, outputLocation );
                        }
                    }
                }
            }
        }
    }
}
