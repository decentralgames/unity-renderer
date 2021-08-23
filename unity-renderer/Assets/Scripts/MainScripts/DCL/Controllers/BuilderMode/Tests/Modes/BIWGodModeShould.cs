using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Camera;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWGodModeShould : IntegrationTestSuite_Legacy
{
    private BIWModeController modeController;
    private BIWRaycastController raycastController;
    private BIWGizmosController gizmosController;
    private BIWGodMode godMode;
    private BIWContext context;
    private GameObject mockedGameObject, entityGameObject;
    private List<BIWEntity> selectedEntities;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        modeController = new BIWModeController();
        raycastController = new BIWRaycastController();
        gizmosController = new BIWGizmosController();
        selectedEntities = new List<BIWEntity>();
        context = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
            modeController,
            raycastController,
            gizmosController,
            InitialSceneReferences.i
        );

        mockedGameObject = new GameObject("MockedGameObject");
        entityGameObject = new GameObject("EntityGameObject");
        modeController.Init(context);
        raycastController.Init(context);
        gizmosController.Init(context);
        modeController.EnterEditMode(scene);
        raycastController.EnterEditMode(scene);
        gizmosController.EnterEditMode(scene);

        godMode =  (BIWGodMode) modeController.GetCurrentMode();
        godMode.SetEditorReferences(mockedGameObject, mockedGameObject, mockedGameObject, mockedGameObject, selectedEntities);
        godMode.freeCameraController = Substitute.For<IFreeCameraMovement>();
        godMode.freeCameraController.Configure().gameObject.Returns(mockedGameObject);
    }

    [Test]
    public void GodModeActivation()
    {
        //Act
        godMode.Activate(scene);

        //Assert
        Assert.IsTrue(godMode.IsActive());
    }

    [Test]
    public void GodModeDeactivation()
    {
        //Arrange
        godMode.Activate(scene);

        //Act
        godMode.Deactivate();

        //Assert
        Assert.IsFalse(godMode.IsActive());
    }

    [Test]
    public void TestNewObject()
    {
        //Arrange
        BIWEntity newEntity = new BIWEntity();
        newEntity.Init(TestHelpers.CreateSceneEntity(scene), null);
        raycastController.RayCastFloor(out Vector3 floorPosition);

        //Act
        modeController.CreatedEntity(newEntity);

        //Assert
        Assert.IsTrue(Vector3.Distance(mockedGameObject.transform.position, floorPosition) <= 0.001f);
    }

    [Test]
    public void ChangeTemporarySnapActive()
    {
        //Arrange
        modeController.SetSnapActive(false);
        selectedEntities.Add(new BIWEntity());

        //Act
        context.inputsReferencesAsset.multiSelectionInputAction.RaiseOnStarted();

        //Assert
        Assert.IsTrue(godMode.isSnapActive);
    }

    [Test]
    public void ChangeTemporarySnapDeactivated()
    {
        //Arrange
        modeController.SetSnapActive(false);
        selectedEntities.Add(new BIWEntity());
        context.inputsReferencesAsset.multiSelectionInputAction.RaiseOnStarted();

        //Act
        context.inputsReferencesAsset.multiSelectionInputAction.RaiseOnFinished();

        //Assert
        Assert.IsFalse(godMode.isSnapActive);
    }

    [Test]
    public void TranslateGizmos()
    {
        //Arrange
        godMode.RotateMode();

        //Act
        godMode.TranslateMode();

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.TRANSLATE_GIZMO_NAME);
    }

    [Test]
    public void RotateGizmos()
    {
        //Arrange
        godMode.TranslateMode();

        //Act
        godMode.RotateMode();

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.ROTATE_GIZMO_NAME);
    }

    [Test]
    public void ScaleGizmos()
    {
        //Arrange
        godMode.TranslateMode();

        //Act
        godMode.ScaleMode();

        //Assert
        Assert.AreEqual(gizmosController.GetSelectedGizmo(), BIWSettings.SCALE_GIZMO_NAME);
    }

    [Test]
    public void EntityTransformByGizmos()
    {
        //Arrange
        godMode.RotateMode();
        var entity = new BIWEntity();
        selectedEntities.Add(entity);

        var rotationToApply = new Vector3(0, 180, 0);

        //Act
        godMode.EntitiesTransfromByGizmos(rotationToApply);

        //Assert
        Assert.AreEqual(entity.GetEulerRotation(), rotationToApply);
    }

    [Test]
    public void UpdateSelectionPosition()
    {
        //Arrange
        godMode.TranslateMode();
        var biwEntity = new BIWEntity();
        var newEntity = Substitute.For<IDCLEntity>();
        newEntity.Configure().gameObject.Returns(entityGameObject);
        biwEntity.Init(newEntity, null);
        selectedEntities.Add(biwEntity);

        var newPosition = new Vector3(10, 10, 10);

        //Act
        godMode.UpdateSelectionPosition(newPosition);

        //Assert
        Assert.AreEqual(mockedGameObject.transform.position,  WorldStateUtils.ConvertSceneToUnityPosition(newPosition, scene));
    }

    [Test]
    public void UpdateSelectionRotation()
    {
        //Arrange
        godMode.RotateMode();
        var biwEntity = new BIWEntity();
        var newEntity = Substitute.For<IDCLEntity>();
        newEntity.Configure().gameObject.Returns(entityGameObject);
        biwEntity.Init(newEntity, null);
        selectedEntities.Add(biwEntity);

        var rotation = new Vector3(10, 10, 10);

        //Act
        godMode.UpdateSelectionRotation(rotation);

        //Assert
        Assert.IsTrue(Vector3.Distance(entityGameObject.transform.rotation.eulerAngles,  rotation) <= 0.01f);
    }

    [Test]
    public void UpdateSelectionScale()
    {
        //Arrange
        godMode.ScaleMode();
        var biwEntity = new BIWEntity();
        var newEntity = Substitute.For<IDCLEntity>();
        newEntity.Configure().gameObject.Returns(entityGameObject);
        biwEntity.Init(newEntity, null);
        selectedEntities.Add(biwEntity);

        var scale = new Vector3(10, 10, 10);

        //Act
        godMode.UpdateSelectionScale(scale);

        //Assert
        Assert.AreEqual(entityGameObject.transform.localScale,  scale);
    }

    [Test]
    public void MouseClick()
    {
        //Arrange
        godMode.isPlacingNewObject = true;

        //Act
        godMode.MouseClickDetected();

        //Assert
        context.entityHandler.Received().DeselectEntities();
    }

    [Test]
    public void DragEditionGameObject()
    {
        //Arrange
        godMode.dragStartedPoint = Vector3.zero;
        var mousePosition = new Vector3(100, 100, 100);
        var initialGameObjectPosition = mockedGameObject.transform.position;

        //Act
        godMode.DragEditionGameObject(mousePosition);

        //Assert
        Assert.AreNotEqual(initialGameObjectPosition, mockedGameObject.transform.position);
    }

    [Test]
    public void MouseUpOnUIStopDrag()
    {
        //Arrange
        godMode.isMouseDragging = true;

        //Act
        godMode.OnInputMouseUpOnUi(0, Vector3.zero);

        //Assert
        Assert.IsFalse(godMode.isMouseDragging);
    }

    [Test]
    public void MouseUpStopDrag()
    {
        //Arrange
        godMode.isMouseDragging = true;

        //Act
        godMode.OnInputMouseUp(0, Vector3.zero);

        //Assert
        Assert.IsFalse(godMode.isMouseDragging);
    }

    [Test]
    public void MousePressed()
    {
        //Arrange
        godMode.mouseMainBtnPressed = false;
        godMode.isPlacingNewObject = false;

        //Act
        godMode.OnInputMouseDown(0, Vector3.zero);

        //Assert
        Assert.IsTrue(godMode.mouseMainBtnPressed);
    }

    [Test]
    public void StartDraggingEntities()
    {
        //Arrange
        godMode.TranslateMode();
        var biwEntity = new BIWEntity();
        var newEntity = Substitute.For<IDCLEntity>();
        newEntity.Configure().gameObject.Returns(entityGameObject);
        biwEntity.Init(newEntity, null);
        selectedEntities.Add(biwEntity);
        context.entityHandler.Configure().IsPointerInSelectedEntity().Returns(true);

        //Act
        godMode.StarDraggingSelectedEntities();

        //Assert
        Assert.IsTrue(godMode.isDraggingStarted);
    }

    [Test]
    public void EndDraggingEntities()
    {
        //Arrange
        godMode.TranslateMode();
        var biwEntity = new BIWEntity();
        var newEntity = Substitute.For<IDCLEntity>();
        newEntity.Configure().gameObject.Returns(entityGameObject);
        biwEntity.Init(newEntity, null);
        selectedEntities.Add(biwEntity);
        godMode.StarDraggingSelectedEntities();

        //Act
        godMode.EndDraggingSelectedEntities();

        //Assert
        Assert.IsFalse(godMode.isDraggingStarted);
    }

    [Test]
    public void MouseDrag()
    {
        //Arrange
        godMode.lastMousePosition = new Vector3(180, 180, 180);
        godMode.canDragSelectedEntities = true;
        var mousePosition = new Vector3(0, 0, 0);

        //Act
        godMode.OnInputMouseDrag(0, mousePosition, 0, 0);

        //Assert
        Assert.IsTrue(godMode.isMouseDragging);
    }

    [Test]
    public void DeselectedEntities()
    {
        //Arrange
        var biwEntity = new BIWEntity();
        var newEntity = Substitute.For<IDCLEntity>();
        newEntity.Configure().gameObject.Returns(entityGameObject);
        biwEntity.Init(newEntity, null);
        selectedEntities.Add(biwEntity);
        godMode.isPlacingNewObject = true;

        //Act
        godMode.EntityDeselected(biwEntity);

        //Assert
        Assert.IsFalse(godMode.isPlacingNewObject);
    }

    [Test]
    public void LookAtEntity()
    {
        //Arrange
        var newEntity = Substitute.For<IDCLEntity>();
        newEntity.Configure().gameObject.Returns(entityGameObject);
        var position = Vector3.zero;

        //Act
        godMode.LookAtEntity(newEntity);

        //Assert
        godMode.freeCameraController.Received().SmoothLookAt(position);
    }

    [UnityTest]
    public IEnumerator CalculateEntityMidPoint()
    {
        //Arrange
        var entity = TestHelpers.CreateSceneEntity(scene);

        TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

        TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
            }));
        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
        yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
        yield return null;

        //Act
        var postion = godMode.CalculateEntityMidPoint(entity);

        //Assert
        Assert.AreEqual(postion, entity.renderers[0].bounds.center);
    }

    [Test]
    public void FocusGameObject()
    {
        //Arrange
        var biwEntity = new BIWEntity();
        var newEntity = Substitute.For<IDCLEntity>();
        newEntity.Configure().gameObject.Returns(entityGameObject);
        biwEntity.Init(newEntity, null);
        selectedEntities.Add(biwEntity);

        //Act
        godMode.FocusEntities(selectedEntities);

        //Assert
        godMode.freeCameraController.Received().FocusOnEntities(selectedEntities);
    }

    [Test]
    public void ResetCamera()
    {
        //Act
        godMode.ResetCamera();

        //Assert
        godMode.freeCameraController.Received().ResetCameraPosition();
    }

    [Test]
    public void TakeScreenshotForPublish()
    {
        //Act
        godMode.TakeSceneScreenshotForPublish();

        //Assert
        godMode.freeCameraController.Received().TakeSceneScreenshot(Arg.Any<IFreeCameraMovement.OnSnapshotsReady>());
    }

    [Test]
    public void TakeScreenshotForExit()
    {
        //Act
        godMode.TakeSceneScreenshotForExit();

        //Assert
        godMode.freeCameraController.Received().TakeSceneScreenshotFromResetPosition(Arg.Any<IFreeCameraMovement.OnSnapshotsReady>());
    }

    [Test]
    public void OpenNewProjectDetails()
    {
        //Act
        godMode.OpenNewProjectDetails();

        //Assert
        godMode.freeCameraController.Received().TakeSceneScreenshot(Arg.Any<IFreeCameraMovement.OnSnapshotsReady>());
    }

    protected override IEnumerator TearDown()
    {
        context.inputsReferencesAsset.multiSelectionInputAction.RaiseOnFinished();
        raycastController.Dispose();
        modeController.Dispose();
        gizmosController.Dispose();

        context.Dispose();

        GameObject.Destroy(mockedGameObject);
        GameObject.Destroy(entityGameObject);
        return base.TearDown();
    }
}