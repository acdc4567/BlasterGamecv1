using UnityEngine;
using System.Collections;
using TPC.CameraScripts;
using Manager;
using TPC.Items;

namespace TPC
{
    public class InputHandler : MonoBehaviour
    {
        [HideInInspector]
        public StateManager states;
        [HideInInspector]
        public CameraManager camManager;
        HandleMovement_Player hMove;
        CrosshairManager crosshair;
        Controller_Extras cExtras;

        float horizontal;
        float vertical;
        bool runInput;
        bool aimInput;
        float shootAxis;
        float aimAxis;
        bool shootInput;
        bool reloadInput;
        bool action1Input;
        bool action2Input;
        bool switchInput;
        bool firstPerson;
        bool pivotInput;
        bool vaultInput;
        bool coverInput;
        bool crouchInput;
        bool pickupInput;
        
        public Vector3 aimPosition;
        [HideInInspector]
        public Vector3 coverNormal;

        [Header("Add a value to this")]
        public AnimationCurve vaultCurve;
        Renderer[] modelRenderers;

        UIManager uiM;
        UI.CanvasOverlay lvlCanvas;

        cState camState;
        enum cState { fps, tps }

        PickableItem itemToPickup;
        bool canPickup;

        //v2
        public static InputHandler singleton;

        void Start()
        {
            singleton = this;
            uiM = UIManager.singleton;
            lvlCanvas = UI.CanvasOverlay.singleton;
            //Add references
            gameObject.AddComponent<HandleMovement_Player>();
            gameObject.AddComponent<Controller_Extras>();

            //Get references
            crosshair = CrosshairManager.singleton;
            camManager = CameraManager.singleton;
            camManager.target = this.transform;

            states = GetComponent<StateManager>();
            hMove = GetComponent<HandleMovement_Player>();
            cExtras = GetComponent<Controller_Extras>();

            camManager.target = this.transform;
            camManager.transform.position = transform.position;
            camManager.states = states;
            //Init in order
            states.isPlayer = true;

            if (!SessionMaster.singleton.debugMode)
            {
                PlayerProfile p = Manager.SessionMaster.singleton.GetProfile();
                CharContainer charContainer = ResourcesManager.singleton.GetChar(p.charId);
                GameObject model = charContainer.prefab;
                states.modelPrefab = model;
                states.modelRig = charContainer.rig;
                states.Init();
                states.weaponManager.weapons = new System.Collections.Generic.List<string>();
                states.weaponManager.weapons.Add(p.mainWeapon);
                states.weaponManager.weapons.Add(p.secWeapon);
            }
            else
            {
                states.Init();
            }

            states.weaponManager.Init(states); //The weapon manager needs to initialize after we set our weapons

            hMove.Init(states,this);
            cExtras.Init(states,this);

            FixPlayerMeshes();

            //v2
            Camera.main.cullingMask = ~(1 << LayerMask.NameToLayer("WeaponMods"));
        }

        void FixPlayerMeshes()
        {
            modelRenderers = states.activeModel.GetComponentsInChildren<Renderer>();
           
            SkinnedMeshRenderer[] smr = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer r in smr)
            {
                r.updateWhenOffscreen = true;
            }
        }

        void FixedUpdate()
        {
            if (states.isDead)
                return;

            states.FixedTick();         
          
            if(!states.inCover)
                hMove.Tick();

            states.ikHandler.Tick();
        }

        bool initForMenu;

        void HandleInGameMenu()
        {
            if (uiM.gameMenu_UI.activeInHierarchy)
            {
                if (!initForMenu)
                {
                    camManager.enabled = false;
                    lvlCanvas.gameObject.SetActive(false);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    if (!SessionMaster.singleton.isMultiplayer)
                    {
                        Time.timeScale = 0;
                    }
                    initForMenu = true;
                }
            }
            else
            {
                if (initForMenu)
                {
                    lvlCanvas.gameObject.SetActive(true);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    camManager.enabled = true;

                    if (!SessionMaster.singleton.isMultiplayer)
                    {
                        Time.timeScale = 1;
                    }
                    initForMenu = false;
                }
            }

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                uiM.gameMenu_UI.SetActive(!uiM.gameMenu_UI.activeInHierarchy);
            }

            HandleInGameMenu();

            //v2
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                if (!initMod)
                    EnableWeaponMod();
                else
                    DisableWeaponMod();
            }

            if (initForMenu || initMod)
                return;

            if (states.isDead)
                return;

            GetInput();
            HandlePickup();
            UpdateStatesFromInput();
            states.RegularTick();
            HandleAim();
            CameraChanger();
            HandleCrosshair();
            cExtras.Tick(camManager.pivot.position);

            //if (Input.GetKeyDown(KeyCode.Space))
           //       states.SubtractHealth(30);

            if (coverInput)
            {
                if (states.canCover && !states.inCover)
                {
                    states.curState = StateManager.CharStates.cover;
                    states.inCover = true;
                    return;     
                }
                else
                {
                    states.curState = StateManager.CharStates.idle;
                    states.inCover = false;
                    states.rBody.isKinematic = false;
                    cExtras.ResetCover();
                }    
            }

            if(vaultInput)
            {
                float distanceFromCover = Vector3.Distance(transform.position, states.startVaultPosition);

                if(distanceFromCover < 1.5f && states.canVault && states.canVault_b)
                {
                    if (states.inCover)
                    {
                        hMove.vaultFromCover = true;
                        states.curState = StateManager.CharStates.idle;
                        states.inCover = false;
                        cExtras.ResetCover();
                    }

                    states.vaulting = true;
                    states.skipGroundCheck = true;      
                }
            }
        }

        //v2
        bool initMod;
        public void EnableWeaponMod()
        {
            if (initMod == false)
            {
                initMod = true;
                uiM.weaponModUI.SetActive(true);
                lvlCanvas.gameObject.SetActive(false);
                Weapons.Modifications.CarryingWeapons carrWeapons = new Weapons.Modifications.CarryingWeapons();

                //Primary rifles etc, should be the 0, secondary should be pistols etc.  
                carrWeapons.primary = states.weaponManager.weaponReferences[0].GetModContainer();
                carrWeapons.secondary = states.weaponManager.weaponReferences[1].GetModContainer();
                carrWeapons.primary.weaponId = states.weaponManager.weapons[0];
                carrWeapons.secondary.weaponId = states.weaponManager.weapons[1];
                        
                uiM.modSceneReferences.activeCategory = states.weaponManager.GetActive().wReference.weaponCategory;
                uiM.modSceneReferences.gameObject.SetActive(true);
                uiM.modSceneReferences.Init(carrWeapons);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Time.timeScale = 0;
                Camera.main.cullingMask = (1 << LayerMask.NameToLayer("WeaponMods"));
            }
        }

        public void DisableWeaponMod()
        {
            if (initMod)
            {
                initMod = false;

                //reset the camera
                camManager.camActual.parent = camManager.camTrans;
                camManager.camActual.localPosition = Vector3.zero;
                camManager.camActual.localRotation = Quaternion.identity;

                //take any changes we did on our weapons
                Weapons.Modifications.CarryingWeapons currentCarrying = uiM.modSceneReferences.CloseWeaponModUI();
                //close everything
                uiM.weaponModUI.SetActive(false);
                lvlCanvas.gameObject.SetActive(true);
                uiM.modSceneReferences.gameObject.SetActive(false);

                //reset state for game
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Camera.main.cullingMask = ~(1 << LayerMask.NameToLayer("WeaponMods"));
                   
                states.weaponManager.ReplaceWeapon(currentCarrying);

                PlayerProfile pl = SessionMaster.singleton.GetProfile();
                pl.mainWeapon = states.weaponManager.weapons[0];
                pl.mainWeaponMods = states.weaponManager.weaponReferences[0].modelReferences.activeMods;
                pl.secWeapon = states.weaponManager.weapons[1];
                pl.secWeaponMods = states.weaponManager.weaponReferences[1].modelReferences.activeMods;
                SessionMaster.singleton.SaveProfile();
            }
        }

        void LateUpdate()
        {
            if (states.isDead)
                return;

            states.LateTick();
            states.ikHandler.LateTick();
        }

        void GetInput()
        {
            if(Input.GetButtonDown(Statics.firstPersonInput))
            {
                firstPerson = !firstPerson;
            }

            vertical = Input.GetAxis(Statics.Vertical);
            horizontal = Input.GetAxis(Statics.Horizontal);
            aimAxis = Input.GetAxis(Statics.aimInput);
            aimInput = (aimAxis != 0);
            shootAxis = Input.GetAxis(Statics.shootInput);
            runInput = Input.GetButton(Statics.runInput);
            reloadInput = Input.GetButton(Statics.reloadInput);
            switchInput = Input.GetButton(Statics.switchInput);
            vaultInput = Input.GetButton(Statics.vaultInput);
            coverInput = Input.GetButtonUp(Statics.coverInput);
            crouchInput = Input.GetButtonDown(Statics.crouchInput);
            pickupInput = Input.GetKeyUp(KeyCode.Z);

            if (!states.onLocomotion)
                aimInput = false;
            if (states.inCover)
                runInput = false;
            
        }

        void CameraChanger()
        {
            if(firstPerson)
            {
                if(states.inAction || !states.aiming)
                {
                    if (camState != cState.tps)
                    {
                        CameraChangeState(false);
                    }
                }
                else
                {
                    if (camState != cState.fps)
                    {
                        CameraChangeState(true);
                    }
                } 
            }
            else
            {
                if (camState != cState.tps)
                {
                    CameraChangeState(false);
                }
            }
        }

        void CameraChangeState(bool fps)
        {
            if(fps)
            {
                camManager.camActual.transform.parent = states.ikHandler.aimPivot;
                Vector3 targetPos = Vector3.zero;
                targetPos = states.weaponManager.GetActive().wReference.weaponStats.fps_camera_offset;
                camManager.camActual.transform.localPosition = targetPos;
                camManager.camActual.transform.localRotation = Quaternion.identity;
                camManager.SetCameraValuesToFps();
                MeshesStatus(false);
                camState = cState.fps;
            }
            else
            {
                camManager.camActual.transform.parent = camManager.camTrans;
                camManager.camActual.transform.localPosition = Vector3.zero;
                camManager.camActual.transform.localRotation = Quaternion.identity;
                camManager.SetCameraValuesToTps();
                MeshesStatus(true);
                camState = cState.tps;
            }
        }

        void MeshesStatus(bool status)
        {
            foreach (Renderer r in modelRenderers)
            {
                r.enabled = status;
            }
        }

        void UpdateStatesFromInput()
        {         
            Vector3 v = camManager.transform.forward * vertical;
            Vector3 h = camManager.transform.right * horizontal;

            v.y = 0;
            h.y = 0;

            states.horizontal = horizontal;
            states.vertical = vertical;
            Vector3 moveDir = (h + v).normalized;
            states.moveDirection = moveDir;
            states.reload = reloadInput;
            states.switchWeapon = switchInput;
            states.onLocomotion = states.anim.GetBool(Statics.onLocomotion);

            if (!states.shooting)
                states.aiming = aimInput;

            if (states.aiming)
            {
                if (states.inCover && !states.inCoverCanAim)
                {
                    states.aiming = false;
                }
            }

            if (states.reloading)
                states.aiming = false;

            if (!states.aiming)
            {            
                states.inAngle_MoveDir = InAngle(states.moveDirection, 25);
                if (states.walk && horizontal != 0 || states.walk && vertical != 0)
                {
                    states.inAngle_MoveDir = InAngle(states.moveDirection, 60);
                }

                HandleRun();    
            }
            else
            {
                states.canRun_b = false;
                states.walk = true;
                states.inAngle_MoveDir = true;
            }

            if (crouchInput && !states.run)
                states.crouching = !states.crouching;
        }

        void HandleAim()
        {
            if(!states.vaulting)
                states.anim.SetBool(Statics.aim, states.aiming);
            else
            {
                states.anim.SetBool(Statics.aim, false);
                return;
            }

            Ray ray = new Ray(camManager.camTrans.position, camManager.camTrans.forward);
            Debug.DrawRay(ray.origin,ray.direction*5);
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit, 50,states.ignoreLayers))
            {
                aimPosition = hit.point;
            }
            else
            {
                aimPosition = ray.GetPoint(25);
            }

            states.aimPosition = aimPosition;

            if (states.aiming && !states.inAction)
            {
                camManager.ChangeState(Statics.aim);
                Weapons.Weapon w = states.weaponManager.GetActive().wReference;
                camManager.SetSpeed(w.weaponStats.turnSpeed, w.weaponStats.turnSpeedController);

                Vector3 dir = aimPosition - transform.position;
                float angle = Vector3.Angle(transform.forward, dir);
                states.inAngle_Aim = (angle < 30);

                shootInput = (shootAxis != 0) && !states.inAction;
                states.shooting = shootInput;
            }
            else
            {
                camManager.SetDefault();
                camManager.ChangeState(Statics.normal);
                states.inAngle_Aim = false;
                shootInput = false;
                states.shooting = false;
            }

            if (states.actualShooting)
            {
                Weapons.WeaponStats activeStats = states.weaponManager.GetActive().activeStats;
                camManager.SetOffsets(activeStats.cameraRecoilX, activeStats.cameraRecoilY);
            }
        }

        bool InAngle(Vector3 targetDir , float angleTheshold)
        {
            bool r = false;
            float angle = Vector3.Angle(transform.forward, targetDir);

            if (angle < angleTheshold)
            {
                r = true;
            }

            return r;
        }

        void HandleRun()
        {
            if (runInput)
            {
                states.walk = false;
                states.run = true;
                states.crouching = false;
            }
            else
            {
                states.walk = true;
                states.run = false;
            }

            if (horizontal != 0 || vertical != 0)
            {
                states.run = runInput;
                states.anim.SetInteger(Statics.specialType, 
                    Statics.GetAnimSpecialType(AnimSpecials.run));
            }
            else
            {
                if(states.run)
                    states.run = false;
            }

            if (!states.inAngle_MoveDir && hMove.doAngleCheck)
                states.run = false;

            if (states.obstacleForward)
                states.run = false;

            if (states.run == false)
            {
                states.anim.SetInteger(Statics.specialType, Statics.GetAnimSpecialType(AnimSpecials.runToStop));
            }
        }

        void HandleCrosshair()
        {
            bool rotate = false;

            if (states.run)
                rotate = true;
            if (states.inAction)
                rotate = true;

            if (rotate)
            {
                crosshair.activeCrosshair.rotateCursor(1);
                crosshair.activeCrosshair.WiggleCrosshair();
            }
        }

        public void EnableRootMovement()
        {
            hMove.EnableRootMovement();
        }

        void HandlePickup()
        {
            if(!states.inAction)
            {
                if(canPickup && pickupInput)
                {
                    itemToPickup.PickupItem(states);
                }
            }
        }

        public void CanPickupItem(PickableItem target)
        {
            itemToPickup = target;
            canPickup = true;
        }

        public void DisablePickupItem()
        {
            itemToPickup = null;
            canPickup = false;
        }
    }
}
