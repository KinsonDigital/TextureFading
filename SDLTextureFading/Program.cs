using SDLCore;
using System;
using FileIO.File;
using FileIO.Core;
using System.IO;
using System.Reflection;
using IOFile = FileIO.File.File;
using System.Diagnostics;
using SDLCore.Structs;

namespace SDLTextureFading
{
    public class Programp
    {
        private static string _contentPath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Content";
        private static readonly IFile _libFile = new IOFile();
        private static readonly IPlatform _platform = new Platform();
        private static SDL _sdl;
        private static SDLImage _sdlImage;
        private static SDLFonts _sdlFonts;
        private static SDLMixer _sdlMixer;

        static void Main(string[] args)
        {
            IDependencyManager sdlDependencyManager = new SDLDependencyManager(_platform, _libFile);
            IDependencyManager sdlImageDependencyManager = new SDLImageDependencyManager(_platform, _libFile);
            IDependencyManager sdlMixerDependencyManager = new SDLMixerDependencyManager(_platform, _libFile);
            IDependencyManager sdlFontsDependencyManager = new SDLFontsDependencyManager(_platform, _libFile);

            //This is an example of how the library resolver can be used
            _sdl = new SDL(new LibraryLoader(sdlDependencyManager, _platform, _libFile, "SDL2.dll"));
            _sdlImage = new SDLImage(new LibraryLoader(sdlImageDependencyManager, _platform, _libFile, "SDL2_image.dll"));
            _sdlMixer = new SDLMixer(new LibraryLoader(sdlMixerDependencyManager, _platform, _libFile, "SDL2_mixer.dll"));
            _sdlFonts = new SDLFonts(new LibraryLoader(sdlFontsDependencyManager, _platform, _libFile, "SDL2_ttf.dll"));

            //Check to make sure that the video card can be initialized
            if (_sdl.Init(SDL.InitVideo) < 0)
            {
                Console.WriteLine("Unable to initialize SDL. Error: {0}", _sdl.GetError());
            }
            else
            {
                //Create a window
                var windowPtr = _sdl.CreateWindow(".NET Core SDL2-CS Tutorial",
                    SDL.WindowPosCentered,
                    SDL.WindowPosCentered,
                    1020,
                    800,
                    WindowFlags.WindowResizable
                );

                var linkTexturePath = $@"{_contentPath}\Link.png";
                var dungeonBackgroundPath = $@"{_contentPath}\Dungeon.png";

                //Create a renderer for rendering graphics to the screen
                var rendererPtr = _sdl.CreateRenderer(windowPtr, -1, RendererFlags.RendererAccelerated);

                //Load the textures
                var linkTexturePtr = _sdlImage.LoadTexture(rendererPtr, linkTexturePath);
                var dungeonTexturePtr = _sdlImage.LoadTexture(rendererPtr, dungeonBackgroundPath);

                var error = _sdl.GetError();

                if (error != string.Empty)
                    Debugger.Break();

                //Create the rectangles for the dungeon
                Rect dungeonSrcRect;//The section of the image to render
                dungeonSrcRect.x = 0;
                dungeonSrcRect.y = 0;
                dungeonSrcRect.w = 1020;
                dungeonSrcRect.h = 800;

                Rect dungeonTargetRect;//The location on the surface of where to render the image
                dungeonTargetRect.x = 0;
                dungeonTargetRect.y = 0;
                dungeonTargetRect.w = 1020;
                dungeonTargetRect.h = 800;

                //Create the rectangles for link
                Rect linkSrcRect;//The section of the image to render
                linkSrcRect.x = 0;
                linkSrcRect.y = 0;
                linkSrcRect.w = 92;
                linkSrcRect.h = 112;

                Rect linkTargetRect;//The location on the surface of where to render the image
                linkTargetRect.x = 450;
                linkTargetRect.y = 300;
                linkTargetRect.w = 92;
                linkTargetRect.h = 112;

                if (windowPtr == IntPtr.Zero)
                {
                    Console.WriteLine("Unable to create a window. _sdl. Error: {0}", _sdl.GetError());
                }
                else
                {

                    bool quit = false;

                    while (!quit)
                    {
                        while (_sdl.PollEvent(out var e) != 0)
                        {
                            //Check for which type event the window has thrown
                            switch (e.type)
                            {
                                case EventType.Quit://Quit app event
                                    quit = true;
                                    break;
                                case EventType.KeyDown://Key event
                                    //Check for various key input
                                    switch (e.key.keysym.sym)
                                    {
                                        case KeyCode.q:
                                            quit = true;
                                            break;
                                    }
                                    break;
                            }
                        }

                        //Clear the rendering surface, which is the window
                        _sdl.RenderClear(rendererPtr);

                        //Set the blend mode and alpha of link to be transparent
                        _sdl.SetTextureBlendMode(linkTexturePtr, BlendModes.Blend);
                        _sdl.SetTextureAlphaMod(linkTexturePtr, 200);

                        //Render the dungeon background

                        //Render dungeon
                        _sdl.RenderCopy(rendererPtr, dungeonTexturePtr, ref dungeonSrcRect, ref dungeonTargetRect);//Copies the texture onto the render surface.  It is not visible at this point

                        //Render link
                        _sdl.RenderCopy(rendererPtr, linkTexturePtr, ref linkSrcRect, ref linkTargetRect);//Copies the texture onto the render surface.  It is not visible at this point

                        //Actually display the results onto the rendering texture
                        _sdl.RenderPresent(rendererPtr);
                    }
                }

                //Destroy the texture, renderer and window
                _sdl.DestroyTexture(rendererPtr);
                _sdl.DestroyRenderer(rendererPtr);
                _sdl.DestroyWindow(windowPtr);

                //And quit the application
                _sdl.Quit();
            }
        }
    }
}
