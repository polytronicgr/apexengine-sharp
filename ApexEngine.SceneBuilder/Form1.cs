﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModernUISample.metro;
using ApexEngine.Scene;
using ApexEngine.Assets;
using ApexEngine.Rendering;
using ApexEditor.NormalMapGenerator;
using ApexEngine.Scene.Components;
using ApexEngine.Math;

namespace ApexEditor
{
    public partial class Form1 : Form
    {
        ApexEngineControl apxCtrl;
        private int activeNodeID;
        frmMatEditor matEditor;
        private ApexEngine.Rendering.Shadows.ShadowMappingComponent shadowCpt;
        private SceneEditorGame.CamModes camMode = SceneEditorGame.CamModes.Freelook;



        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("Apex Editor Started.");
            matEditor = new frmMatEditor();
        }
        void Style_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DarkStyle")
            {
                BackColor = MetroUI.Style.BackColor;
                Refresh();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (MetroUI.DesignMode == false)
            {
                MetroUI.Style.PropertyChanged += Style_PropertyChanged;
                MetroUI.Style.DarkStyle = true;
            }
            ImageList ilist1 = new ImageList();
            ilist1.Images.Add(Properties.Resources.node_16);
            ilist1.Images.Add(Properties.Resources.geometry_16);
            ilist1.Images.Add(Properties.Resources.material);
            treeView1.ImageList = ilist1;

            SceneEditorGame game = new SceneEditorGame();
            //  game.Camera = new ApexEngine.Rendering.Cameras.DefaultCamera(game.InputManager, 75);
            game.Camera.Translation = new ApexEngine.Math.Vector3f(0, 2, 0);
            game.Camera.Enabled = false;
            apxCtrl = new ApexEngineControl(game);
            apxCtrl.Dock = DockStyle.Fill;
            pnlGameView.Controls.Add(apxCtrl);


            contextMenuStrip1.Renderer = new metroToolStripRenderer();

            

            PopulateTreeView(game.RootNode);
            /*

            SceneEditorGame orthoTop = new SceneEditorGame();
          //  orthoTop.Camera = new ApexEngine.Rendering.Cameras.OrthoCamera(-5, 5, -5, 5, -5, 5);
            orthoTop.Camera.Translation = new ApexEngine.Math.Vector3f(0, 0, -5);
           // orthoTop.RenderManager.GeometryList = game.RenderManager.GeometryList;
            ApexEngineControl orthoTopCtrl = new ApexEngineControl(orthoTop);
            orthoTopCtrl.Dock = DockStyle.Fill;
            pnlOrthoTop.Controls.Add(orthoTopCtrl);*/

        }
        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        private void AddTreeViewItem(TreeNode parent, GameObject obj)
        {
            if (obj is Geometry)
            {
                TreeNode newNode = new TreeNode(obj.Name + " (Geometry)");
                Geometry geom = (Geometry)obj;
                newNode.Tag = geom;
                TreeNode matNode = new TreeNode(geom.Material.GetName());
                matNode.Tag = geom.Material;
                newNode.Nodes.Add(matNode);
                newNode.ImageIndex = 1;
                if (parent == null)
                    treeView1.Nodes.Add(newNode);
                else
                    parent.Nodes.Add(newNode);
            }
            else if (obj is Node)
            {
                TreeNode newNode = new TreeNode(obj.Name + " (Node)");
                Node n = (Node)obj;
                newNode.Tag = n;
                newNode.ImageIndex = 0;
                if (parent == null)
                    treeView1.Nodes.Add(newNode);
                else
                    parent.Nodes.Add(newNode);
                foreach (GameObject g in n.Children)
                {
                    AddTreeViewItem(newNode, g);
                }
            }

        }
        private void PopulateTreeView(GameObject rootObject)
        {
            AddTreeViewItem(null, rootObject);
        }

        private void ReloadComponents()
        {
            listBox1.Items.Clear();
            foreach (GameComponent gc in apxCtrl.Game.GameComponents)
            {
                listBox1.Items.Add(gc);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }

        private void addToSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the dialog and get result.
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                ApexEngine.Scene.GameObject loadedModel = ApexEngine.Assets.AssetManager.LoadModel(openFileDialog1.FileName);
                apxCtrl.Game.RootNode.AddChild(loadedModel);
               // List<Geometry> geoms = ApexEngine.Rendering.Util.MeshUtil.GatherGeometry(loadedModel);
               // foreach (Geometry g in geoms)
                    apxCtrl.Game.PhysicsWorld.AddObject(loadedModel, 0f, ApexEngine.Scene.Physics.PhysicsWorld.PhysicsShape.Box);
                activeNodeID = apxCtrl.Game.RootNode.Children.Count - 1;
                AddTreeViewItem(treeView1.Nodes[0], loadedModel);
            }
        }

        private void openAsSeperateSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the dialog and get result.
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                List<GameObject> objs = ApexEngine.Rendering.Util.MeshUtil.GatherObjects(apxCtrl.Game.RootNode);
                foreach (GameObject g in objs)
                    apxCtrl.Game.PhysicsWorld.RemoveObject(g);

                for (int i = apxCtrl.Game.RootNode.Children.Count - 1; i > -1; i--)
                    apxCtrl.Game.RootNode.RemoveChild(apxCtrl.Game.RootNode.GetChild(i));
                treeView1.Nodes.Clear();
                ApexEngine.Scene.GameObject loadedModel = ApexEngine.Assets.AssetManager.LoadModel(openFileDialog1.FileName);
                apxCtrl.Game.RootNode.AddChild(loadedModel);
                apxCtrl.Game.PhysicsWorld.AddObject(loadedModel, 0f, ApexEngine.Scene.Physics.PhysicsWorld.PhysicsShape.Box);
                activeNodeID = apxCtrl.Game.RootNode.Children.Count - 1;
                PopulateTreeView(apxCtrl.Game.RootNode);
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                GameObject[] objs = new GameObject[apxCtrl.Game.RootNode.Children.Count];
                for (int i = 0; i < objs.Length; i++)
                {
                    objs[i] = apxCtrl.Game.RootNode.GetChild(i);
                }
                ApexEngine.Assets.Apx.ApxExporter.ExportModel(saveFileDialog1.FileName, objs);
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                if (e.Node.Tag is GameObject)
                {
                    propertyGrid1.SelectedObject = e.Node.Tag;
                }
            }
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
           
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Tag is GameObject)
                {
                    if (treeView1.SelectedNode.Tag != apxCtrl.Game.RootNode)
                    {
                        GameObject selectedObj = (GameObject)treeView1.SelectedNode.Tag;
                        if (selectedObj.HasController(typeof(ApexEngine.Scene.Physics.RigidBodyControl)))
                        {
                            apxCtrl.Game.PhysicsWorld.RemoveObject(selectedObj);
                        }
                        if (selectedObj is Node)
                        {
                            List<GameObject> childObjs = ApexEngine.Rendering.Util.MeshUtil.GatherObjects(selectedObj);
                            foreach (GameObject g in childObjs)
                                apxCtrl.Game.PhysicsWorld.RemoveObject(g);
                        }
                        selectedObj.GetParent().RemoveChild(selectedObj);
                        treeView1.Nodes.Remove(treeView1.SelectedNode);
                    }
                }
                else if (treeView1.SelectedNode.Tag is Material)
                {
                    Geometry selectedObj = (Geometry)(treeView1.SelectedNode.Parent.Tag);
                    selectedObj.Material = new Material();
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Tag == apxCtrl.Game.RootNode)
                {
                    contextMenuStrip1.Items[0].Enabled = false;
                }
                else
                {
                    contextMenuStrip1.Items[0].Enabled = true;

                }
                if (treeView1.SelectedNode.Tag is Geometry)
                {
                    setToOriginToolStripMenuItem.Enabled = true;
                }
                else
                {
                    setToOriginToolStripMenuItem.Enabled = false;
                }

                if (treeView1.SelectedNode.Tag is Node)
                {
                    lockToolStripMenuItem.Enabled = true;
                    if (((Node)treeView1.SelectedNode.Tag).HasController(typeof(ApexEngine.Scene.Physics.RigidBodyControl)))
                        lockToolStripMenuItem.Checked = true;
                    else
                        lockToolStripMenuItem.Checked = false;
                }
                else
                {
                    lockToolStripMenuItem.Enabled = false;
                    lockToolStripMenuItem.Checked = false;
                }
            }
        }

        private void contextMenuStrip1_Opened(object sender, EventArgs e)
        {
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Tag != apxCtrl.Game.RootNode)
                {
                    GameObject selectedObj = (GameObject)treeView1.SelectedNode.Tag;
                    saveFileDialog1.FileName = selectedObj.Name;
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        ApexEngine.Assets.Apx.ApxExporter.ExportModel(saveFileDialog1.FileName, selectedObj);
                    }
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                GameObject[] objs = new GameObject[apxCtrl.Game.RootNode.Children.Count];
                for (int i = 0; i < objs.Length; i++)
                {
                    objs[i] = apxCtrl.Game.RootNode.GetChild(i);
                }
                Console.WriteLine(objs.Length);
                ApexEngine.Assets.Apx.ApxExporter.ExportModel(saveFileDialog1.FileName, objs);
            }
        }

        private void pnlMtl_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlMtl_DoubleClick(object sender, EventArgs e)
        {
            matEditor.ShowDialog();
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Material)
            {
                frmMatEditor matEdit = new frmMatEditor();
                matEdit.Init();
                matEdit.Material = (Material)treeView1.SelectedNode.Tag;
                matEdit.Show();
                if (matEdit.DialogResult == DialogResult.OK)
                {
                    treeView1.SelectedNode.Tag = matEdit.Material;
                }
            }
        }

        private void generateCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new CodeGen().Show();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameTest test = new GameTest();
            test.RootNode = this.apxCtrl.Game.RootNode;
            test.Run();
        }

        private void normalMapGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new NrmGenerator().Show();
        }

        private void metroMenuStrip5_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            addCpts cptDlg = new addCpts(apxCtrl.Game);
            if (cptDlg.ShowDialog() == DialogResult.OK)
            {
                apxCtrl.Game.AddComponent(cptDlg.resComponent);
                ReloadComponents();
                treeView1.Nodes.Clear();
                PopulateTreeView(apxCtrl.Game.RootNode);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReloadComponents();
            treeView1.Nodes.Clear();
            PopulateTreeView(apxCtrl.Game.RootNode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                apxCtrl.Game.RemoveComponent((GameComponent)listBox1.SelectedItem);
                ReloadComponents();
                treeView1.Nodes.Clear();
                PopulateTreeView(apxCtrl.Game.RootNode);
            }
            catch (Exception ex) { }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {

            PopulateTreeView(apxCtrl.Game.RootNode);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            PopulateTreeView(apxCtrl.Game.RootNode);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void renderWireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((SceneEditorGame)apxCtrl.Game).RenderDebug = renderWireframeToolStripMenuItem.Checked;
        }

        private void shadowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if (shadowsToolStripMenuItem.Checked)
            {
                apxCtrl.Game.RenderManager.AddComponent((shadowCpt == null ? shadowCpt = new ApexEngine.Rendering.Shadows.ShadowMappingComponent(apxCtrl.Game.Camera, apxCtrl.Game.Environment) : shadowCpt));
            }
            else
            {
                apxCtrl.Game.RenderManager.RemoveComponent(shadowCpt);
            }*/
        }

        private void setToOriginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Geometry geom = (Geometry)treeView1.SelectedNode.Tag;
            ApexEngine.Rendering.Util.MeshUtil.SetToOrigin(geom);
        }

        private void lockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameObject go = (GameObject)treeView1.SelectedNode.Tag;
            if (go is Node)
            {
                if (!lockToolStripMenuItem.Checked)
                {
                    foreach (GameObject child in ((Node)go).Children)
                    {
                        if (child.HasController(typeof(ApexEngine.Scene.Physics.RigidBodyControl)))
                        {
                            apxCtrl.Game.PhysicsWorld.RemoveObject(child);
                        }
                    }
                    if (!go.HasController(typeof(ApexEngine.Scene.Physics.RigidBodyControl)))
                    {
                        apxCtrl.Game.PhysicsWorld.AddObject(go, 0.0f, ApexEngine.Scene.Physics.PhysicsWorld.PhysicsShape.Box);
                    }
                }
                else
                {
                    if (go.HasController(typeof(ApexEngine.Scene.Physics.RigidBodyControl)))
                    {
                        apxCtrl.Game.PhysicsWorld.RemoveObject(go);
                    }
                    foreach (GameObject child in ((Node)go).Children)
                    {
                        if (!child.HasController(typeof(ApexEngine.Scene.Physics.RigidBodyControl)))
                        {
                            apxCtrl.Game.PhysicsWorld.AddObject(child, 0.0f, ApexEngine.Scene.Physics.PhysicsWorld.PhysicsShape.Box);
                        }
                    }
                }
            }
        }

        private void SetCamMode(SceneEditorGame.CamModes camMode)
        {
            this.camMode = camMode;
            checkBox1.Checked = (camMode == SceneEditorGame.CamModes.Freelook);
            checkBox2.Checked = (camMode == SceneEditorGame.CamModes.Grab);
            checkBox3.Checked = (camMode == SceneEditorGame.CamModes.Rotate);
            ((SceneEditorGame)apxCtrl.Game).CamMode = camMode;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            SetCamMode(SceneEditorGame.CamModes.Freelook);
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            SetCamMode(SceneEditorGame.CamModes.Grab);
        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            SetCamMode(SceneEditorGame.CamModes.Rotate);
        }
    }
}
