this is what I thought good code looked like seven years ago
please forgive me for my sins

I thought about deleting these but, y'know what, if you're curious you can take a look.

OLD README:

# Hello!
I'm Nicholas "Kritz" Blackburn. Here's some code I've written!
Most of these samples are small stand-alone methods. Since I down own exclusive rights to the code here, some files might be missing some minor context.

# Short Descriptions of Files

1. RailObjectTracker.cs AND RailCamera.cs - Gameplay camera that uses complex math and calculations to track unpredictable multiplayer racing car movement. Camera needs to track all players and keep them all on-screen within the same camera bounds. Uses a mix of animation curves for smoothing, z-forward offsetting and height offsetting, a fallback top-down camera state, dynamic camera swaying when taking tight corners.

You can view the camera in action here: https://youtu.be/XIjPv5cZeJ0?t=63 
Note: this video wasn't recorded by myself or Giant Margharita, and isn't curated in any way. I haven't watched all the way through but I don't believe there's any voice commentary over the video so it's hopefully safe for work. :)

2. ActionBehaviours.cs - A very small, simple file showing familiarity with making designer-friendly variable definitions using Unity editor calls. In this example, most commenting is replaced with Tooltips.
Note: This has been moved to ./Archive/

3. BounceReflection.cs - Some small gameplay math to bounce a ball off a surface normal. For various reasons, Unity's physics engine wasn't suitable for this specific interaction, and I was tasked with writing a behaviour that allowed a similar behaviour.
Note: You might be able to see this behaviour in the above Party Crashers gameplay whenever a plasma ball (the purple pickup) bounces off a track wall. This functionality only works when the plasma ball has been "powered up" so sometimes it will just explode on hit.

4. InputHelper.cs - A utility function to provide quick and easy Screen-to-world and Screen-to-UI raycasts.

5. LerpAnimation.cs - A utility function to get eased lerp values.
Note: This has been moved to ./Archive/

6. Spiral.cs - A slightly heavier math function to create a "phone-cord" style spiral pattern using LineRenderer vertices.

# Coding Styles

I'm updating this readme in 2023 so I couldn't say how accurate this is, but I tend to stick pretty closely to the MSDN C# coding style guide,
with some exceptions made for making Unity dev easier.

I tend to use the Rider IDE and find it matches my preferred style.

# Licensing
MIT License
