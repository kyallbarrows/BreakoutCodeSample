## Running the app
Open with Unity 2019.4.29, and use mouse for control, or watch a playthru video here: https://youtu.be/mCcF9loifqo

## QA Status and known issues
- I haven't done a full QA pass due to time constraints.  Scoring and losing lives works, but I haven't tested losing all lives, or clearing all the bricks.
- The hit detection, and thus the physics, has a pretty serious problem: a hit on the corner pixel of a brick can send the ball in the wrong direction.  This can cause it to bounce between bricks and take out a whole swath at once.  It's fixable, but I'm out of time.

## Concessions to expediency (aka "I'd do it if I had more time")
- I'm skipping over the initial state of the game. It's not quite accurate to how it worked on the Atari 2600, but it's also hard to figure out how the start screen worked without playing on device.  Instead, I'm just launching the ball immediately.
- I'm using float values for the ball velocity.  Floating point math was almost certainly not possible on an Atari 2600, but how they fudged the physics with integers is a bit unclear to me, so I'm "fixing" it.
- The AI player is not really AI, and probably won't ever win the game.  But it won't lose either!  It just stays underneath the ball.
- The sprites should really be atlased, but it makes reading in the pixel arrays a little more complex.  Almost need a third party tool and a JSON reader instance.
- The texture is still rendering somewhat fuzzy. I turned sampling to Point in the texture import, but there's probably another setting I'm missing.  If all else failed, I suppose I could change the DrawBuffer class to use a much larger texture, and draw every pixel as an X by X square.  However, although I'd like it crisp to match the low-poly vibe of the room scene, the fuzziness kind of works here, since old tv's were anything but crisp.
- There are multiple other concessions, marked with //TODO:
- "Movies are never finished, only abandoned." -- George Lucas

## Design decisions
I'm not really worrying about non-power-of-2 texture sizes.  I can't seem to find good consensus on the internet as to whether it's still important for mobile, although it appears that Unity will store them at the next power of 2 up, so it's wasting a little bit of storage.  It works fine on an iPad mini 3 running iOS 14, but if it caused issues on Android or other iOS devices, I'd probably atlas the textures with an external tool (not Unity's internal one, since that seems to be made mostly for SpriteRenderers) and change the part where I read the pixel data into the dictionary.

The spec mentioned "Visualization: Graphics with Unity3D preferred".  The author may have intended sprites or 3d bricks.  Instead, I did a rather complex and possibly unnecessary method of building out a Texture2D using array copying.  The reason is that I'd really like to bulge the tv screen like TV's of the era had, although I ran out of time.  This is easy with a UV-mapped texture, but would be difficult to pull off with 2D sprites (although using a second camera and a RenderTexture that then copies to the Texture2D might be an option, haven't tried it yet). 

In the DrawBuffer class, simple SetPixel calls would've sufficed, but I went a bit overboard because this is a code sample, and in samples it's ok to over-optimize when you don't really need it, just to show how you might approach a larger-scale issue of similar structure.  In this instance, there would only be about 2000 SetPixel calls, which nearly any modern device would handle just fine at 30fps or more, so all the memory copying is totally unneeded.  But it sure is fun!

## Changes from original
The original hit detection was a bit weird.  I've watched YouTube playthrus extensively, but haven't quite figured out what they were doing.  I know that high ball speeds could cause it to warp through bricks, but the specifics of which way the ball bounces when this happens are still eluding me.  Since we have better processors available now, I'm "improving" it by doing pixel-by-pixel physics and hit detection with the ball.  However, it has the issues mentioned above.