using ARKit;
using SceneKit;
using System;
using UIKit;

namespace XamarinArkitSample
{
    public partial class ViewController : UIViewController
    {
        private readonly ARSCNView sceneView;

        public ViewController(IntPtr handle) : base(handle)
        {
            this.sceneView = new ARSCNView
            {
                AutoenablesDefaultLighting = true,
                Delegate = new SceneViewDelegate()
            };

            this.View.AddSubview(this.sceneView);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.sceneView.Frame = this.View.Frame;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            var detectionImages = ARReferenceImage.GetReferenceImagesInGroup("AR Resources", null);

            this.sceneView.Session.Run(new ARWorldTrackingConfiguration
            {
                AutoFocusEnabled = true,
                PlaneDetection = ARPlaneDetection.Horizontal | ARPlaneDetection.Vertical,
                LightEstimationEnabled = true,
                WorldAlignment = ARWorldAlignment.Gravity,
                DetectionImages = detectionImages,
                MaximumNumberOfTrackedImages = 1

            }, ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            this.sceneView.Session.Pause();
        }
    }

    public class SceneViewDelegate : ARSCNViewDelegate
    {
        public override void DidAddNode(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
        {
            // Patient Identifier image detected in scene 
            if (anchor is ARImageAnchor imageAnchor)
            {
                var detectedImage = imageAnchor.ReferenceImage;

                // If were going to detect and show more than one patient,
                // we would determine which  relevant patient identified was detected here

                ShowPatientDetails(detectedImage, node);
            }
        }



        private void ShowPatientDetails(ARReferenceImage detectedImage, SCNNode node)
        {


            // Highlight the QR Code?
            var width = detectedImage.PhysicalSize.Width;
            var length = detectedImage.PhysicalSize.Height;
            var planeNode = new PlaneNode(0, width, length, new SCNVector3(0, 0, 0), "");
            float angle = (float)(-Math.PI / 2);
            planeNode.EulerAngles = new SCNVector3(angle, 0, 0);
            node.AddChildNode(planeNode);

            // Get and Show information panels
            foreach (var informationPanelNode in GetPatientInformationPanels())
            {
                var waitAction = SCNAction.Wait(0.1 * informationPanelNode.Number);
                var fadeInAction = SCNAction.FadeIn(1);
                var actionSequence = SCNAction.Sequence(new[] { waitAction, fadeInAction });

                // Not sure I can run actions before adding. May have to add, then run.
                informationPanelNode.RunAction(actionSequence); 

                node.AddChildNode(informationPanelNode);
            }
        }

        private PlaneNode[] GetPatientInformationPanels()
        {
            return new PlaneNode[]
            {
                // Need the team to provide the images
                new PlaneNode(1, 300, 600, new SCNVector3(-500, 500, 0), "Images/test panel 02.png"),
                new PlaneNode(2, 300, 600, new SCNVector3(-500, 300, 0), "Images/test panel 06.png"),
                new PlaneNode(3, 300, 600, new SCNVector3(-100, 0, 0), "Images/test panel 01.png"),
                new PlaneNode(4, 300, 600, new SCNVector3(100,0,0), "Images/test panel 07.png"),
                new PlaneNode(5, 300, 600, new SCNVector3(500, 0, 0), "Images/test panel 04.png"),
                new PlaneNode(6, 300, 600, new SCNVector3(500, 300, 0), "Images/test panel 05.png"),
                new PlaneNode(7, 300, 600, new SCNVector3(500, -500, 0), "Images/test panel 07.png"),


            };

        }

       
    }

    public class PlaneNode : SCNNode
    {
        public int Number { get; set; }

        public PlaneNode(int number, nfloat width, nfloat length, SCNVector3 position, string imagePath)
        {
            var rootNode = new SCNNode
            {
                Geometry = CreateGeometry(width, length, imagePath),
                Position = position,
            };

            Number = number;


            AddChildNode(rootNode);
        }

        private static SCNGeometry CreateGeometry(nfloat width, nfloat length, string imagePath)
        {
            var image = UIImage.FromFile(imagePath);

            var material = new SCNMaterial();
            material.Diffuse.Contents = image;
            material.DoubleSided = true;

            var geometry = SCNPlane.Create(width, length);
            geometry.Materials = new[] { material };

            return geometry;
        }
    }
}