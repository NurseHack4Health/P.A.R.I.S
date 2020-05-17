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
            planeNode.Opacity = 1f;
            float angle = (float)(-Math.PI / 2);
            planeNode.EulerAngles = new SCNVector3(angle, 0, 0);
            //node.AddChildNode(planeNode);

            // Get and Show information panels
            foreach (var informationPanelNode in GetPatientInformationPanels())
            {
                var waitAction = SCNAction.Wait(0.1 * informationPanelNode.Number);
                var fadeInAction = SCNAction.FadeIn(1);
                var actionSequence = SCNAction.Sequence(new[] { waitAction, fadeInAction });

                // Not sure I can run actions before adding. May have to add, then run.
                informationPanelNode.RunAction(actionSequence);

                informationPanelNode.EulerAngles = new SCNVector3(angle, 0, 0);

                node.AddChildNode(informationPanelNode);
            }
        }

        private PlaneNode[] GetPatientInformationPanels()
        {
            return new PlaneNode[]
            {
                // Need the team to provide the images
                new PlaneNode(1, 0.3f, 0.5f, new SCNVector3(-0.35f, -0.35f, -0.015f), "Images/vitals.png"), // Vital s
                new PlaneNode(2, 0.3f, 0.6f, new SCNVector3(-0.35f, 0.25f, 0.01f), "Images/neuro-cardiac.png"), // Neuro

                new PlaneNode(3, 0.3f, 0.6f, new SCNVector3(0, 0, 0.01f), "Images/patient-details2.png"), // Center

                new PlaneNode(4, 0.3f, 0.6f, new SCNVector3(0.35f, 0.2f, 0.01f), "Images/gi-gu.png"), // Labs/Radiology
                new PlaneNode(5, 0.3f, 0.5f, new SCNVector3(0.35f, -0.4f, 0.01f), "Images/labs-radiology.png"), // Labs/Radiology

                new PlaneNode(6, 0.3f, 0.6f, new SCNVector3(0.7f, 0.25f, 0.01f), "Images/medications.png"), // Medications
                new PlaneNode(7, 0.3f, 0.6f, new SCNVector3(0.7f, -0.45f, 0.01f), "Images/xrays.png"), // xrays



                //new PlaneNode(5, -0.1f, 0.2f, new SCNVector3(0.3f, 0, 0), "Images/test panel 04.png"), // Right 1
                //new PlaneNode(6, -0.1f, 0.2f, new SCNVector3(0.5f, -0.1f, 0), "Images/test panel 05.png"),  // Right 2
                //new PlaneNode(7, -0.1f, 0.2f, new SCNVector3(0.4f, -0.5f, 0), "Images/test panel 08.png"), // bottom right


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

            Opacity = 0;


            Number = number;


            AddChildNode(rootNode);
        }

        private static SCNGeometry CreateGeometry(nfloat width, nfloat length, string imagePath)
        {
            UIImage image = null;
            var material = new SCNMaterial();

            if (!string.IsNullOrEmpty(imagePath))
            {
                image = UIImage.FromFile(imagePath);
                material.Diffuse.Contents = image;
                //material.Diffuse.ContentColor
            }
            else
            {
                material.Diffuse.Contents = UIColor.White;
            }





            material.DoubleSided = true;

            var geometry = SCNPlane.Create(width, length);
            geometry.Materials = new[] { material };

            return geometry;
        }
    }
}