using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL;
using DCL.Camera;
using DCL.Controllers;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using Environment = DCL.Environment;

public class BIWMainControllerShould : IntegrationTestSuite_Legacy
{
    private BIWMainController mainController;
    private BIWContext context;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        mainController = new BIWMainController();
        context = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
        );
        BIWMainController.BYPASS_LAND_OWNERSHIP_CHECK = true;
        mainController.Initialize();
    }

    [Test]
    public void EnterBuilderInWorld()
    {
        //Arrange
        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        //Act
        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Assert
        Assert.IsTrue(mainController.isBuilderInWorldActivated);
    }

    [Test]
    public void ExitBuilderInWorld()
    {
        //Arrange
        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.ExitEditMode();

        //Assert
        Assert.IsFalse(mainController.isBuilderInWorldActivated);
    }

    [Test]
    public void InitController()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();

        //act 
        mainController.InitController(controller);
        controller.Init(context);

        //Assert
        controller.Received(1).Init(context);
    }

    [Test]
    public void FindSceneToEdit()
    {
        //Arrange
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);
        CommonScriptableObjects.playerWorldPosition.Set(new Vector3(scene.sceneData.basePosition.x, 0, scene.sceneData.basePosition.y));

        //Act
        var sceneFound = mainController.FindSceneToEdit();

        //Arrange
        Assert.AreEqual(scene, sceneFound);
    }

    [Test]
    public void ControllerEnterEditMode()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        //Act
        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Assert
        controller.Received(1).EnterEditMode(scene);
    }

    [Test]
    public void ControllerExitEditMode()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.ExitEditMode();

        //Assert
        controller.Received(1).ExitEditMode();
    }

    [Test]
    public void ControllerOnGUI()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.OnGUI();

        //Assert
        controller.Received(1).OnGUI();
    }

    [Test]
    public void ControllerLateUpdate()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.LateUpdate();

        //Assert
        controller.Received(1).LateUpdate();
    }

    [Test]
    public void ControllerNoActiveReceveingUpdate()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        //Act
        mainController.Update();

        //Assert
        controller.Received(0).Update();
    }

    [Test]
    public void ControllerUpdate()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        mainController.CatalogLoaded();
        scene.CreateEntity("Test");

        mainController.TryStartEnterEditMode(false, scene, "Test");
        ParcelScene createdScene = (ParcelScene) Environment.i.world.sceneController.CreateTestScene(scene.sceneData);
        createdScene.CreateEntity("TestEntiy");
        Environment.i.world.sceneController.SendSceneReady(scene.sceneData.id);

        //Act
        mainController.Update();

        //Assert
        controller.Received(1).Update();
    }

    [Test]
    public void ControllerDispose()
    {
        //Arrange
        IBIWController controller = Substitute.For<IBIWController>();
        mainController.InitController(controller);

        //Act
        mainController.Dispose();

        //Assert
        controller.Received(1).Dispose();
    }

    [Test]
    public void BIWExitWhenCharacterIsFarAway()
    {
        //Arrange
        mainController.sceneToEdit = scene;
        mainController.isBuilderInWorldActivated = true;
        mainController.checkerInsideSceneOptimizationCounter = 60;
        DCLCharacterController.i.characterPosition.unityPosition = Vector3.one * 9999;

        //Act
        mainController.Update();

        //Assert
        Assert.IsFalse(mainController.isBuilderInWorldActivated);
    }

    [Test]
    public void ActivateLandAccessBackground()
    {
        //Arrange
        var profile = UserProfile.GetOwnUserProfile();
        profile.UpdateData(new UserProfileModel() { userId = "testId", ethAddress = "0x00" });

        //Act
        mainController.ActivateLandAccessBackgroundChecker();

        //Assert
        Assert.IsNotNull(mainController.updateLandsWithAcessCoroutine);
    }

    [Test]
    public void RequestCatalog()
    {
        //Arrange
        mainController.isCatalogRequested = false;

        //Act
        mainController.GetCatalog();

        //Assert
        Assert.IsTrue(mainController.isCatalogRequested);
    }

    [Test]
    public void ChangeEditModeByShortcut()
    {
        //Act
        mainController.ChangeEditModeStatusByShortcut(DCLAction_Trigger.BuildEditModeChange);

        //Assert
        Assert.IsTrue(mainController.isWaitingForPermission);
    }

    [Test]
    public void NewSceneAdded()
    {
        //Arrange
        var mockedScene = Substitute.For<IParcelScene>();
        mockedScene.Configure().sceneData.Returns(scene.sceneData);
        mainController.sceneToEditId = scene.sceneData.id;

        //Act
        mainController.NewSceneAdded(mockedScene);

        //Assert
        Assert.AreSame(mainController.sceneToEdit, base.scene);
    }

    [Test]
    public void UserHasPermission()
    {
        //Arrange
        AddSceneToPermissions();

        //Act
        var result = mainController.UserHasPermissionOnParcelScene(scene);

        //Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void IsParcelFromSDK()
    {
        //Arrange
        Parcel parcel = new Parcel();
        parcel.x = scene.sceneData.basePosition.x;
        parcel.y = scene.sceneData.basePosition.y;

        Vector2Int parcelCoords = new Vector2Int(scene.sceneData.basePosition.x, scene.sceneData.basePosition.y);
        Land land = new Land();
        land.parcels = new List<Parcel>() { parcel };

        LandWithAccess landWithAcces = new LandWithAccess(land);
        DeployedScene deployedScene = new DeployedScene();
        deployedScene.parcelsCoord = new Vector2Int[] { parcelCoords };
        deployedScene.deploymentSource = DeployedScene.Source.SDK;

        landWithAcces.scenes = new List<DeployedScene>() { deployedScene };
        var lands = new LandWithAccess[]
        {
            landWithAcces
        };
        DataStore.i.builderInWorld.landsWithAccess.Set(lands);

        //Act
        var result = mainController.IsParcelSceneDeployedFromSDK(scene);

        //Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void CatalogReceived()
    {
        //Arrange

        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/multipleSceneObjectsCatalog.json";
        string jsonValue = File.ReadAllText(jsonPath);

        //Act
        mainController.CatalogReceived(jsonValue);

        //Assert
        Assert.IsTrue(mainController.catalogAdded);
    }

    [Test]
    public void CatalogHeaderReceived()
    {
        //Act
        mainController.CatalogHeadersReceived("");

        //Assert
        Assert.IsTrue(mainController.areCatalogHeadersReady);
    }

    [Test]
    public void CheckSceneToEditByShorcut()
    {
        //Arrange
        mainController.sceneToEdit = scene;
        AddSceneToPermissions();

        //Act
        mainController.CheckSceneToEditByShorcut();

        //Assert
        Assert.IsTrue(mainController.isEnteringEditMode);
    }

    [Test]
    public void InitialLoadingControllerHideOnFloorLoaded()
    {
        //Arrange
        mainController.initialLoadingController.Dispose();
        mainController.initialLoadingController = Substitute.For<IBuilderInWorldLoadingController>();
        mainController.initialLoadingController.Configure().isActive.Returns(true);

        //Act
        mainController.OnAllParcelsFloorLoaded();

        //Assert
        mainController.initialLoadingController.Received().Hide(Arg.Any<bool>(), Arg.Any<Action>());
    }

    [Test]
    public void StartExitModeScreenShot()
    {
        //Arrange
        mainController.modeController.godMode.freeCameraController = Substitute.For<IFreeCameraMovement>();
        mainController.saveController.numberOfSaves = 1;

        //Act
        mainController.StartExitMode();

        //Assert
        mainController.modeController.godMode.freeCameraController.Received().TakeSceneScreenshotFromResetPosition(Arg.Any<IFreeCameraMovement.OnSnapshotsReady>());
    }

    [Test]
    public void SetupNewScene()
    {
        //Arrange
        BIWCatalogManager.Init();
        BIWTestHelper.CreateTestCatalogLocalMultipleFloorObjects();
        mainController.creatorController.sceneToEdit = scene;
        mainController.floorHandler.sceneToEdit = scene;

        //Act
        mainController.SetupNewScene();

        //Assert
        Assert.Greater(mainController.floorHandler.floorPlaceHolderDict.Count, 0);
    }

    [Test]
    public void ExitAfterTeleport()
    {
        //Arrange
        mainController.sceneToEdit = scene;
        mainController.isBuilderInWorldActivated = true;

        //Act
        mainController.ExitAfterCharacterTeleport(new DCLCharacterPosition());

        //Assert
        Assert.IsFalse(mainController.isBuilderInWorldActivated);
    }

    [Test]
    public void ActiveLandCheckerCoroutineAfterUserPofileIsLoaded()
    {
        //Arrange
        var profile = UserProfile.GetOwnUserProfile();
        mainController.ActivateLandAccessBackgroundChecker();
        profile.UpdateData(new UserProfileModel() { userId = "testId", ethAddress = "0x00" });

        //ACt
        mainController.OnUserProfileUpdate(profile);

        //Assert
        Assert.IsNotNull(mainController.updateLandsWithAcessCoroutine);
    }

    private void AddSceneToPermissions()
    {
        var parcel = new Parcel();
        parcel.x = scene.sceneData.basePosition.x;
        parcel.y = scene.sceneData.basePosition.y;
        var land = new Land();
        land.parcels = new List<Parcel>() { parcel };

        var landWithAcces = new LandWithAccess(land);
        landWithAcces.scenes = new List<DeployedScene>();
        var lands = new LandWithAccess[]
        {
            landWithAcces
        };
        DataStore.i.builderInWorld.landsWithAccess.Set(lands);
    }

    protected override IEnumerator TearDown()
    {
        DataStore.i.builderInWorld.catalogItemDict.Clear();
        AssetCatalogBridge.i.ClearCatalog();
        DataStore.i.builderInWorld.landsWithAccess.Set(new LandWithAccess[0]);
        mainController.Dispose();
        BIWMainController.BYPASS_LAND_OWNERSHIP_CHECK = false;
        yield return base.TearDown();
    }
}