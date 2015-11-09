﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ApexEngine.Math;
using ApexEngine.Scene;
using ApexEngine.Rendering;
using ApexEngine.Rendering.Animation;
using ApexEngine.Assets.Util;
namespace ApexEngine.Assets.Apx
{
    public class ApxModelLoader : AssetLoader
    {
        List<Node> nodes = new List<Node>();
        List<Geometry> geoms = new List<Geometry>();
        List<Mesh> meshes = new List<Mesh>();

        List<int> skeletonAssigns = new List<int>();
        List<Skeleton> skeletons = new List<Skeleton>();
        List<List<Bone>> bones = new List<List<Bone>>();
        List<List<BoneAssign>> boneAssigns = new List<List<BoneAssign>>();
        List<Animation> animations = new List<Animation>();
        bool hasAnimations = false;

        List<List<Vector3f>> positions = new List<List<Vector3f>>();
        List<List<Vector3f>> normals = new List<List<Vector3f>>();
        List<List<Vector2f>> texcoords0 = new List<List<Vector2f>>();
        List<List<Vector2f>> texcoords1 = new List<List<Vector2f>>();
        List<List<Vertex>> vertices = new List<List<Vertex>>();
        List<List<int>> faces = new List<List<int>>();

        bool node = false, geom = false;

        Node lastNode = null;
        private void EndModel()
        {
            Skeleton skeleton = skeletons[skeletons.Count - 1];
            if (skeleton.GetNumBones() > 0)
            {
                for (int i = 0; i < skeleton.GetNumBones(); i++)
                    skeleton.GetBone(i).SetToBindingPose();
                skeleton.GetBone(0).CalculateBindingRotation();
                skeleton.GetBone(0).CalculateBindingTranslation();
                for (int i = 0; i < skeleton.GetNumBones(); i++)
                {
                    skeleton.GetBone(i).StoreBindingPose();
                    skeleton.GetBone(i).ClearPose();
                }
                skeleton.GetBone(0).UpdateTransform();
            }
            if (skeletons.Count > 0)
            {
                if (hasAnimations)
                {
                    for (int j = 0; j < animations.Count; j++)
                    {
                         skeletons[0].AddAnimation(animations[j]);
                    }
                    nodes[0].AddController(new AnimationController(skeletons[0]));
                }
            }
            for (int i = 0; i < meshes.Count; i++)
            {
                List<Vertex> cVerts = vertices[i];
                List<int> cFaces = faces[i];

                List<Vector3f> cPos = positions[i];
                List<Vector3f> cNorm = normals[i];
                List<Vector2f> tc0 = texcoords0[i];
                List<Vector2f> tc1 = texcoords1[i];
                List<BoneAssign> ba = null;

                Mesh mesh = meshes[i];
                if (boneAssigns.Count > 0)
                {
                    ba = boneAssigns[i];
                }
                int stride = 3;

                if (tc1.Count > 0)
                    stride++;
                for (int j = 0; j < cFaces.Count; j+=stride)
                {
                    Vertex v = new Vertex(cPos[cFaces[j]], tc0[cFaces[j + 2]], cNorm[cFaces[j + 1]]);
                    if (tc1.Count > 0)
                    {
                        mesh.GetAttributes().SetAttribute(VertexAttributes.TEXCOORDS1);
                        v.SetTexCoord1(tc1[cFaces[i + 3]]);
                    }
                    cVerts.Add(v);
                }
                List<int> indexData = new List<int>();
                if (skeletons.Count > 0)
                {
                    mesh.SetSkeleton(skeletons[0]);
                    if (boneAssigns.Count > 0)
                    {
                        for (int j = 0; j < ba.Count; j++)
                        {
                            Vertex v = cVerts[ba[j].GetVertexIndex()];
                            v.AddBoneIndex(ba[j].GetBoneIndex());
                            v.AddBoneWeight(ba[j].GetBoneWeight());
                        }
                    }
                }
                mesh.SetVertices(cVerts);
                if (geoms.Count > 0)
                {
                    Geometry parent = geoms[i];
                    parent.SetMesh(mesh);
                }
            }
        }
        public override object Load(string filePath)
        {
            XmlReader xmlReader = XmlReader.Create(filePath);
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name == ApxExporter.TOKEN_NODE)
                    {
                        node = true;
                        geom = false;
                        string name = xmlReader.GetAttribute(ApxExporter.TOKEN_NAME);
                        Node n = new Node(name);
                        if (lastNode != null)
                            lastNode.AddChild(n);
                        lastNode = n;
                        nodes.Add(n);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_GEOMETRY)
                    {
                        node = false;
                        geom = true;
                        string name = xmlReader.GetAttribute(ApxExporter.TOKEN_NAME);
                        Geometry g = new Geometry();
                        g.SetName(name);
                        if (lastNode != null)
                            lastNode.AddChild(g);
                        geoms.Add(g);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_MESH)
                    {
                        meshes.Add(new Mesh());
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_VERTICES)
                    {
                        List<Vertex> newVList = new List<Vertex>();
                        vertices.Add(newVList);

                        List<Vector3f> newPList = new List<Vector3f>();
                        positions.Add(newPList);

                        List<Vector3f> newNList = new List<Vector3f>();
                        normals.Add(newNList);

                        List<Vector2f> newT0List = new List<Vector2f>();
                        texcoords0.Add(newT0List);

                        List<Vector2f> newT1List = new List<Vector2f>();
                        texcoords1.Add(newT1List);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_POSITION)
                    {
                        List<Vector3f> pos = positions[positions.Count - 1];

                        float x = float.Parse(xmlReader.GetAttribute("x"));
                        float y = float.Parse(xmlReader.GetAttribute("y"));
                        float z = float.Parse(xmlReader.GetAttribute("z"));

                        Vector3f position = new Vector3f(x, y, z);
                        pos.Add(position);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_NORMAL)
                    {
                        List<Vector3f> nor = normals[normals.Count - 1];

                        float x = float.Parse(xmlReader.GetAttribute("x"));
                        float y = float.Parse(xmlReader.GetAttribute("y"));
                        float z = float.Parse(xmlReader.GetAttribute("z"));

                        Vector3f normal = new Vector3f(x, y, z);
                        nor.Add(normal);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_TEXCOORD0)
                    {
                        List<Vector2f> tc0 = texcoords0[texcoords0.Count - 1];

                        float x = float.Parse(xmlReader.GetAttribute("x"));
                        float y = float.Parse(xmlReader.GetAttribute("y"));
                       
                        Vector2f tc = new Vector2f(x, y);
                        tc0.Add(tc);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_TEXCOORD1)
                    {
                        List<Vector2f> tc1 = texcoords1[texcoords1.Count - 1];

                        float x = float.Parse(xmlReader.GetAttribute("x"));
                        float y = float.Parse(xmlReader.GetAttribute("y"));

                        Vector2f tc = new Vector2f(x, y);
                        tc1.Add(tc);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_FACES)
                    {
                        faces.Add(new List<int>());
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_FACE)
                    {
                        List<int> fList = faces[faces.Count - 1];
                        for (int i =  0; i < 3; i++)
                        {
                            string val = xmlReader.GetAttribute("i" + i.ToString());
                            if (val != "")
                            {
                                string[] tokens = val.Split('/');
                                for (int j = 0; j < tokens.Length; j++)
                                {
                                    fList.Add(int.Parse(tokens[j]));
                                }
                            }
                        }
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_SKELETON)
                    {
                        skeletons.Add(new Skeleton());
                        bones.Add(new List<Bone>());
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_SKELETON_ASSIGN)
                    {
                        string assign = xmlReader.GetAttribute(ApxExporter.TOKEN_ID);
                        skeletonAssigns.Add(int.Parse(assign));
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_BONE)
                    {
                        string name = xmlReader.GetAttribute(ApxExporter.TOKEN_NAME);
                        string parent = xmlReader.GetAttribute(ApxExporter.TOKEN_PARENT);
                        Bone bone = new Bone(name);
                        List<Bone> lastBL = bones[bones.Count - 1];
                        if (!string.IsNullOrEmpty(parent))
                        {
                            foreach (Bone b in lastBL)
                            {
                                if (b.GetName() == parent)
                                {
                                    b.AddChild(bone);
                                }
                            }
                        }
                        List<Bone> skel = bones[bones.Count - 1];
                        skel.Add(bone);
                        Skeleton lastSkeleton = skeletons[skeletons.Count - 1];
                        lastSkeleton.AddBone(bone);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_BONE_BINDPOSITION)
                    {
                        float x = float.Parse(xmlReader.GetAttribute("x"));
                        float y = float.Parse(xmlReader.GetAttribute("y"));
                        float z = float.Parse(xmlReader.GetAttribute("z"));

                        Vector3f vec = new Vector3f(x, y, z);
                        List<Bone> skel = bones[bones.Count - 1];
                        if (skel.Count > 0)
                        {
                            Bone lastBone = skel[skel.Count - 1];
                            lastBone.SetBindTranslation(vec);
                        }
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_BONE_BINDROTATION)
                    {
                        float x = float.Parse(xmlReader.GetAttribute("x"));
                        float y = float.Parse(xmlReader.GetAttribute("y"));
                        float z = float.Parse(xmlReader.GetAttribute("z"));
                        float w = float.Parse(xmlReader.GetAttribute("w"));

                        List<Bone> skel = bones[bones.Count - 1];
                        if (skel.Count > 0)
                        {
                            Bone lastBone = skel[skel.Count - 1];
                         //   lastBone.SetBindAxisAngle(new Vector3f(x, y, z), w);
                            lastBone.SetBindRotation(new Quaternion(x, y, z, w));
                        }
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_BONE_ASSIGNS)
                    {
                        boneAssigns.Add(new List<BoneAssign>());
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_BONE_ASSIGN)
                    {
                        int vertIdx = int.Parse(xmlReader.GetAttribute(ApxExporter.TOKEN_VERTEXINDEX));
                        int boneIdx = int.Parse(xmlReader.GetAttribute(ApxExporter.TOKEN_BONEINDEX));
                        float boneWeight = float.Parse(xmlReader.GetAttribute(ApxExporter.TOKEN_BONEWEIGHT));
                        List<BoneAssign> ba = boneAssigns[boneAssigns.Count - 1];
                        ba.Add(new BoneAssign(vertIdx, boneWeight, boneIdx));
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_ANIMATIONS)
                    {
                        hasAnimations = true;
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_ANIMATION)
                    {
                        string name = xmlReader.GetAttribute(ApxExporter.TOKEN_NAME);
                        Animation anim = new Animation(name);
                        animations.Add(anim);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_ANIMATION_TRACK)
                    {
                        string bone = xmlReader.GetAttribute(ApxExporter.TOKEN_BONE);
                        Bone b = skeletons[skeletons.Count - 1].GetBone(bone);
                        if (b != null)
                        {
                            AnimationTrack track = new AnimationTrack(b);
                            animations[animations.Count - 1].AddTrack(track);
                        }
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_KEYFRAME)
                    {
                        float time = float.Parse(xmlReader.GetAttribute(ApxExporter.TOKEN_TIME));

                        Keyframe frame = new Keyframe(time, null, null);
                        Animation canim = animations[animations.Count - 1];
                        AnimationTrack ctrack = canim.GetTrack(canim.GetTracks().Count - 1);
                        ctrack.AddKeyframe(frame);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_KEYFRAME_TRANSLATION)
                    {
                        Animation canim = animations[animations.Count - 1];
                        AnimationTrack ctrack = canim.GetTrack(canim.GetTracks().Count - 1);
                        Keyframe lastFrame = ctrack.frames[ctrack.frames.Count - 1];

                        float x = float.Parse(xmlReader.GetAttribute("x"));
                        float y = float.Parse(xmlReader.GetAttribute("y"));
                        float z = float.Parse(xmlReader.GetAttribute("z"));

                        Vector3f vec = new Vector3f(x, y, z);
                        lastFrame.SetTranslation(vec);
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_KEYFRAME_ROTATION)
                    {
                        Animation canim = animations[animations.Count - 1];
                        AnimationTrack ctrack = canim.GetTrack(canim.GetTracks().Count - 1);
                        Keyframe lastFrame = ctrack.frames[ctrack.frames.Count - 1];

                        float x = float.Parse(xmlReader.GetAttribute("x"));
                        float y = float.Parse(xmlReader.GetAttribute("y"));
                        float z = float.Parse(xmlReader.GetAttribute("z"));
                        float w = float.Parse(xmlReader.GetAttribute("w"));

                        Quaternion rot = new Quaternion(x, y, z, w);
                        lastFrame.SetRotation(rot);
                    }
                } // start element
                else if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    if (xmlReader.Name == ApxExporter.TOKEN_NODE)
                    {
                        if (lastNode != null)
                        {
                            if (lastNode.GetParent() != null)
                            {
                                lastNode = lastNode.GetParent();
                            }
                            else
                            {
                                lastNode = null;
                            }
                        }
                        node = false;
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_GEOMETRY)
                    {
                        geom = false;
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_SKELETON)
                    {
                        
                    }
                    else if (xmlReader.Name == ApxExporter.TOKEN_MODEL)
                    {
                        // end of model, load in meshes
                       EndModel();
                    }
                } // end element
            }
            return nodes[0];
        }
    }
}
