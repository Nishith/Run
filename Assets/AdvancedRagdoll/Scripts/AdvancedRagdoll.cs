using UnityEngine;
using System.Collections;

/*
 * ExampleRagdoll
 * 
 * By Bren Lynne
 * 
 * This script synchs a ragdoll to a specific object's current animation pose 
 * (for seamless transition to ragdoll state),
 * and synchs a specified object to the ragdoll's current pose, 
 * (for seamless transition out of ragdoll state)
 * 
 */

public class AdvancedRagdoll : MonoBehaviour
{
    // debug flag, logs information the console
    public bool debug;

    // the ragdoll's root bone
    public Transform ragdollRoot;

    // a flag to indicate whether materials should also be copied
    public bool copyMaterials;

    public LayerMask groundLayer = -1;
    int groundMask = -1;

    void Start()
    {
        if (debug)
            Debug.Log("AdvancedRagdoll.Start() " + name);

        if (groundLayer != -1)
            groundMask = 1 << groundLayer;
    }

    /*
     * SynchRagdollIn
     * 
     * @param Transform src
     *  The transform to synch the ragdoll to.
     * 
     * @return 
     *  Nothing.
     *
     * Synchs the ragdoll to a transform's current animation pose, for transition to ragdoll state.
     * 
     */
    public void SynchRagdollIn(Transform src)
    {
        // error checking
        if (!src)
        {
            Debug.LogError("AdvancedRagdoll.SynchRagdollIn() " + name + " passed null src!");
            return;
        }

        // copy materials?
        if (copyMaterials)
        {
            CopyMaterials(src, transform);
        }

        // copy transforms
        CopyTransforms(src, transform);
    }

    /*
     * SynchRagdollOut
     * 
     * @param Transform src
     *  The transform to apply the ragdoll's current pose to.
     * 
     * @return 
     *  Nothing.
     *
     * Synchs the specified transform to the ragdoll's current pose, for transition out of ragdoll state.
     * 
     */
    public void SynchRagdollOut(Transform dest)
    {
        // error checking
        if (!dest)
        {
            Debug.LogError("AdvancedRagdoll.SynchRagdollOut() " + name + " passed null dest!");
            return;
        }

        // copy materials?
        if (copyMaterials)
        {
            CopyMaterials(transform, dest);
        }

        // find the ground under the ragdoll's root
        RaycastHit hit;
        Ray ray = new Ray(ragdollRoot.position, -Vector3.up);

        Vector3 snapOffset = Vector3.zero;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (groundMask != -1 ? groundMask : (1 << LayerMask.NameToLayer("Default")))))
        {
            if (debug)
                Debug.Log("AdvancedRagdoll.SynchRagdollOut() " + name + " looking for ground found " + hit.collider.name);

            snapOffset = (ragdollRoot.position - hit.point);

            // snap the ragdoll to the ground
            transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning("AdvancedRagdoll.SynchRagdollOut() " + name + " could not find ground beneath the ragdoll's root!");
        }

        // move the ragdoll's root
        ragdollRoot.position = transform.position + snapOffset;

        // apply ragdoll pose to transform
        PoseToAnimation(transform, dest);
    }

    /*
     * CopyTransforms
     * 
     * @param Transform src
     *  The transform to copy from.
     * 
     * @param Transform dest
     *  The transform to copy to.
     * 
     * @return 
     *  Nothing.
     *
     * Recursively copies a source transform's pos/rot/scale to a destination transform.
     * 
     */
    void CopyTransforms(Transform src, Transform dest)
    {
        // error checking
        if (!src)
        {
            Debug.LogError("AdvancedRagdoll.CopyTransforms() " + name + " passed null src!");
            return;
        }

        if (!dest)
        {
            Debug.LogError("AdvancedRagdoll.CopyTransforms() " + name + " passed null dest!");
            return;
        }

        // copy position
        dest.localPosition = src.localPosition;
        // copy rotation
        dest.localRotation = src.localRotation;
        // copy scale
        dest.localScale = src.localScale;

        // copy children
        foreach (Transform sc in src)
        {
            Transform dc = dest.Find(sc.name);

            if (dc)
                CopyTransforms(sc, dc);
        }
    }

    /*
     * PoseToAnimation
     * 
     * @param Transform src
     *  The transform to generate the animation pose from.
     * 
     * @param Transform dest
     *  The transform to apply the ragdoll pose to as a looping/blending animation.
     * 
     * @return 
     *  Nothing.
     *
     * This functions examines the source transform an create a short looping animation matching the 
     * ragdoll pose, allowing it to be cross faded/blended with another animation.
     * 
     */
    public void PoseToAnimation(Transform src, Transform dest)
    {
        // error checking
        if (!src)
        {
            Debug.LogError("AdvancedRagdoll.PoseToAnimation() " + name + " passed null src!");
            return;
        }

        if (!dest)
        {
            Debug.LogError("AdvancedRagdoll.PoseToAnimation() " + name + " passed null dest!");
            return;
        }

        // if the target transform doesn't have an animation component, add one
        if (!dest.animation)
        {
            dest.gameObject.AddComponent("Animation");
        }

        // copy transforms, initializing the dest transform pose
        CopyTransforms(src, dest);

        // create a new animation clip
        AnimationClip newClip = new AnimationClip();

        // copy the transforms to an animation
        TransformToAnimationCurve(src, src, dest, dest, newClip);

        // add the new ragdoll pose animation clip
        dest.gameObject.animation.AddClip(newClip, "RagdollPose");

        // set the animation to loop
        dest.gameObject.animation["RagdollPose"].wrapMode = WrapMode.Loop;

        // play the animation
        dest.gameObject.animation.Play("RagdollPose");
    }

    /*
     * TransformToAnimationCurve
     * 
     * @param Transform src
     *  The transform to create a curve from.
     * 
     * @param Transform srcRoot
     *  The source transform's root.
     * 
     * @param Transform dest
     *  The transform to create an AnimationCurve for.
     * 
     * @param Transform destRoot
     *  The destination transform's root.
     * 
     * @param AnimationClip clip
     * The AnimationClip to add this AnimationCurve to.
     * 
     * @return 
     *  Nothing.
     *
     * This function recursively create AnimationCurves for a transform, and adds in to the specified AnimationClip.
     * 
     */
    void TransformToAnimationCurve(Transform src, Transform srcRoot, Transform dest, Transform destRoot, AnimationClip clip)
    {
        // error checking
        if (!src)
        {
            Debug.LogError("AdvancedRagdoll.TransformToAnimationClip() " + name + " passed null src!");
            return;
        }

        if (!dest)
        {
            Debug.LogError("AdvancedRagdoll.TransformToAnimationClip() " + name + " passed null dest!");
            return;
        }

        if (!clip)
        {
            Debug.LogError("AdvancedRagdoll.TransformToAnimationClip() " + name + " passed null clip!");
            return;
        }

        // the path from root to src transform
        string srcPath = GetTransformPathToRoot(src, srcRoot);
        // the path from root to dest transform
        string destPath = GetTransformPathToRoot(dest, destRoot);

        if (debug)
            Debug.Log("AdvancedRagdoll.TransformToAnimationClip() " + name + "\n srcPath = " + srcPath + "\n destPath = " + destPath);

        // create position curve
        AnimationCurve x_pos = AnimationCurve.Linear(0, src.localPosition.x, 1, src.localPosition.x);
        AnimationCurve y_pos = AnimationCurve.Linear(0, src.localPosition.y, 1, src.localPosition.y);
        AnimationCurve z_pos = AnimationCurve.Linear(0, src.localPosition.z, 1, src.localPosition.z);

        // create rotation curve
        AnimationCurve w_rot = AnimationCurve.Linear(0, src.localRotation.w, 1, src.localRotation.w);
        AnimationCurve x_rot = AnimationCurve.Linear(0, src.localRotation.x, 1, src.localRotation.x);
        AnimationCurve y_rot = AnimationCurve.Linear(0, src.localRotation.y, 1, src.localRotation.y);
        AnimationCurve z_rot = AnimationCurve.Linear(0, src.localRotation.z, 1, src.localRotation.z);

        // create scale curve
        AnimationCurve x_scale = AnimationCurve.Linear(0, src.localScale.x, 1, src.localScale.x);
        AnimationCurve y_scale = AnimationCurve.Linear(0, src.localScale.y, 1, src.localScale.y);
        AnimationCurve z_scale = AnimationCurve.Linear(0, src.localScale.z, 1, src.localScale.z);

        // set position curve
        clip.SetCurve(destPath, typeof(Transform), "localPosition.x", x_pos);
        clip.SetCurve(destPath, typeof(Transform), "localPosition.y", y_pos);
        clip.SetCurve(destPath, typeof(Transform), "localPosition.z", z_pos);

        // set rotation curve
        clip.SetCurve(destPath, typeof(Transform), "localRotation.w", w_rot);
        clip.SetCurve(destPath, typeof(Transform), "localRotation.x", x_rot);
        clip.SetCurve(destPath, typeof(Transform), "localRotation.y", y_rot);
        clip.SetCurve(destPath, typeof(Transform), "localRotation.z", z_rot);

        // set scale curve
        clip.SetCurve(destPath, typeof(Transform), "localScale.x", x_scale);
        clip.SetCurve(destPath, typeof(Transform), "localScale.y", y_scale);
        clip.SetCurve(destPath, typeof(Transform), "localScale.z", z_scale);

        // do children
        foreach (Transform sc in src)
        {
            Transform dc = dest.Find(sc.name);

            if (dc)
                TransformToAnimationCurve(sc, srcRoot, dc, destRoot, clip);
        }

    }

    /*
     * GetTransformPathToRoot
     * 
     * @param Transform t
     *  The transform to get the path for.
     * 
     * @param Transform root
     *  The root transform, where the path begins.
     * 
     * @return string
     *  The path from root to t.
     * 
     * This function returns the path from the root Transform to the specified Transform.
     * 
     */
    string GetTransformPathToRoot(Transform t, Transform root)
    {
        // error checking
        if (!t)
        {
            Debug.LogError("AdvancedRagdoll.GetTransformPathToRoot() " + name + " passed null t!");
            return "";
        }

        if (!root)
        {
            Debug.LogError("AdvancedRagdoll.GetTransformPathToRoot() " + name + " passed null root!");
            return "";
        }

        // the path to return, beginning with t Transform
        string path = t.name;

        // the first parent
        Transform c_parent = t.parent;

        // go through each parent up the tree until we reach the specified root
        while (c_parent != null && c_parent != root)
        {
            path = path.Insert(0, c_parent.name + "/");
            c_parent = c_parent.parent;
        }

        // return the path
        return path;
    }

    /*
     * CopyMaterials
     * 
     * @param Transform src
     *  The transform to copy materials from.
     * 
     * @param Transform dest
     *  The transform to copy materials to.
     * 
     * @return 
     *  Nothing.
     * 
     * This function copies materials from the src Transform to the dest Transform.
     * 
     */
    void CopyMaterials(Transform src, Transform dest)
    {
        // error checking
        if (!src)
        {
            Debug.LogError("AdvancedRagdoll.CopyMaterials() " + name + " passed null src!");
            return;
        }

        if (!dest)
        {
            Debug.LogError("AdvancedRagdoll.CopyMaterials() " + name + " passed null dest!");
            return;
        }

        // get the src renderers
        Renderer[] src_renderers = src.GetComponentsInChildren<Renderer>();
        // get the dest renderers
        Renderer[] dest_renderers = dest.GetComponentsInChildren<Renderer>();

        // make sure src and dest have the same number of renderers
        if (src_renderers.Length != dest_renderers.Length)
        {
            Debug.LogError("AdvancedRagdoll.CopyMaterials() " + name + " number of src renderers (" + src_renderers.Length + ") does not match dest (" + dest_renderers.Length + ")");
        }
        else if (src_renderers.Length == 0)
        {
            Debug.LogError("AdvancedRagdoll.CopyMaterials() " + name + " has no renderers!");
        }
        else
        {
            // copy materials from src renderers to dest renderers
            for (int r = 0; r < src_renderers.Length; ++r)
            {
                dest_renderers[r].materials = src_renderers[r].materials;
            }
        }
    }

}
