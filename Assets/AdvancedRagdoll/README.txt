
AdvancedRagdoll script for Unity 3

By Bren Lynne
iam@brenlynne.com

USAGE
-----

See ExampleScript.cs for an example of how to use the AdvancedRagdoll script.

Put the AdvancedRagdoll.cs on a ragdoll prefab.

When you create the ragdoll for an animated mesh, at the mesh's position and rotation, 
then call AdvancedRagdoll.SynchRagdollIn, passing the object you want the ragdoll to synch to.  
This will put the ragdoll into a pose based on the current animation pose of the animated mesh.
Then, destroy the animated mesh and let the ragdoll do its thing.

When you are ready to restore the animated mesh, create the "standing up" animated mesh at the 
position and rotation of the ragdoll, and then call AdvanceRagdoll.SynchRagdollOut, 
passing the object you want to synch with the Ragdoll.  This will create a 1 second looping animation 
called "RagdollPose" that matches the ragdoll's current pose, which is automatically added and 
played on the target animated mesh.  CrossFade to a keyed animation to "restore" the animated mesh.  
Then, destroy the ragdoll object.

If you want materials to be copied to/from the ragdoll as well, check the Copy Materials flag on 
your AdvancedRagdoll prefab.

FOR BEST RESULTS
----------------

Create your ragdoll from a mesh in a T-pose.  This creates the best ragdoll.

Make "StandUpFromFaceUp" and "StandUpFromFaceDown" animations, and CrossFade from the ragdoll to 
the appropriate animation, determining which to use with code something like the following:

// find the root bone on your ragdoll
Transform r_root = ragdoll.Find("Root");

if (r_root.forward.y > 0f)
{
    // ragdoll is face up
    character.animation.CrossFade("StandUpFromFaceUp");
}
else
{
    // ragdoll is face down
    character.animation.CrossFade("StandUpFromFaceDown");
}

AdvancedRagdoll attempts to snap the resulting ragdoll pose to the ground, so the synched mesh
will be in the right position following a long fall down stairs, for example.  If you use a layer
to identify the world/ground, you can specify this layer in the ragdoll's "Ground Layer", otherwise, 
it will cast against all layers, which can result in "raised" meshes if, for example, the check hits 
part of the ragdoll itself.

CONCLUSION
----------

Thanks for checking out AdvancedRagdoll, I hope you get some good use out of it!

Comments/feedback are welcome!  Send to the email address at the top of this file.


