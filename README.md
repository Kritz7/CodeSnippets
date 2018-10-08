# Hello!
I'm Nicholas "Kritz" Blackburn. Here's some code I've written!
Most of these samples are small stand-alone methods. Since I down own exclusive rights to the code here, some files might be missing some minor context.

# Short Descriptions of Files

1. RailObjectTracker.cs AND RailCamera.cs - Gameplay camera that uses complex math and calculations to track unpredictable multiplayer racing car movement. Camera needs to track all players and keep them all on-screen within the same camera bounds. Uses a mix of animation curves for smoothing, z-forward offsetting and height offsetting, a fallback top-down camera state, dynamic camera swaying when taking tight corners.

2. ActionBehaviours.cs - A very small, simple file showing familiarity with making designer-friendly variable definitions using Unity editor calls. In this example, most commenting is replaced with Tooltips.

3. BounceReflection.cs - Some small gameplay math to bounce a ball off a surface normal. For various reasons, Unity's physics engine wasn't suitable for this specific interaction, and I was tasked with writing a behaviour that allowed a similar behaviour.

4. InputHelper.cs - A utility function to provide quick and easy Screen-to-world and Screen-to-UI raycasts.

5. LerpAnimation.cs - A utility function to get eased lerp values.

6. Spiral.cs - A slightly heavier math function to create a "phone-cord" style spiral pattern using LineRenderer vertices.

# Commenting Styles

Different teams have different perspectives on code commenting, and I basically just follow convention in these cases.

My preferred Unity style of commenting atm is `/// <<summary>> HelloWorld </summary>`, and in general I try to use big clear method+variable names.

# Licensing
Project intentionally has no license. Sorry. :)
