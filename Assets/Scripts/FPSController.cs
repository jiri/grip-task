using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour {
    public World world;
    new private Transform camera;

    
    public float mouseSensitivity = 10.0f;
    public float walkSpeed = 3.0f;
    public float jumpForce = 5.0f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;

    private Vector3 velocity;
    private float verticalMomentum;
    private bool isGrounded;
    private bool shouldJump;

    public float reach = 3.0f;
    private Vector3Int? placePosition;
    private Vector3Int? destroyPosition;

    public GameObject destroyMarker;
    public GameObject placeMarker;

    public byte selectedBlock;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        this.camera = GameObject.Find("Main Camera").transform;
    }

    void Update() {
        GetInput();

        this.placeMarker.SetActive(this.placePosition != null);
        if (this.placePosition.HasValue) {
            this.placeMarker.transform.position = this.placePosition.Value;
        }

        this.destroyMarker.SetActive(this.destroyPosition != null);
        if (this.destroyPosition.HasValue) {
            this.destroyMarker.transform.position = this.destroyPosition.Value;
        }
    }

    void GetInput() {
        this.horizontal = Input.GetAxis("Horizontal");
        this.vertical = Input.GetAxis("Vertical");

        this.mouseHorizontal += Input.GetAxis("Mouse X") * this.mouseSensitivity;
        this.mouseVertical += Input.GetAxis("Mouse Y") * this.mouseSensitivity;

        this.mouseVertical = Mathf.Clamp(this.mouseVertical, -90.0f, 90.0f);

        if (this.isGrounded && Input.GetButtonDown("Jump")) {
            this.shouldJump = true;
        }

        if (this.destroyPosition.HasValue && Input.GetMouseButtonDown(0)) {
            world.ChunkFromPosition(this.destroyPosition.Value).EditVoxel(this.destroyPosition.Value, 0);
        }

        if (this.placePosition.HasValue && Input.GetMouseButtonDown(1)) {
            world.ChunkFromPosition(this.placePosition.Value).EditVoxel(this.placePosition.Value, this.selectedBlock);
        }
    }

    void FixedUpdate() {
        CalculateVelocity();

        if (shouldJump) {
            shouldJump = false;
            Jump();
        }

        this.transform.rotation = Quaternion.AngleAxis(this.mouseHorizontal, Vector3.up);
        this.camera.rotation = this.transform.rotation * Quaternion.AngleAxis(-this.mouseVertical, Vector3.right);
        transform.Translate(velocity, Space.World);

        RayCast();
    }

    // FIXME: Fails to set placePosition if first step hits (probably ok)
    void RayCast() {
        float stepIncrement = 0.1f;
        float step = stepIncrement;

        Vector3Int lastPosition = new Vector3Int();

        this.destroyPosition = null;
        this.placePosition = null;

        while (step < reach) {
            Vector3 pos = this.camera.position + this.camera.forward * step;

            if (this.world.CheckVoxel(pos)) {
                this.destroyPosition = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

                // Prevents placing blocks diagonally
                Vector3Int newPos = this.destroyPosition.Value;
                if ((newPos.x == lastPosition.x && newPos.y == lastPosition.y) ||
                    (newPos.x == lastPosition.x && newPos.z == lastPosition.z) ||
                    (newPos.y == lastPosition.y && newPos.z == lastPosition.z)) {
                    this.placePosition = lastPosition;
                }
                else {
                    this.placePosition = null;
                }

                break;
            }

            lastPosition = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += stepIncrement;
        }
    }

    void CalculateVelocity() {
        if (this.verticalMomentum > this.gravity) {
            this.verticalMomentum += Time.fixedDeltaTime * this.gravity;
        }

        velocity = (transform.forward * this.vertical + transform.right * this.horizontal) * Time.fixedDeltaTime * this.walkSpeed;
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && this.front) || (velocity.z < 0 && this.back)) {
            velocity.z = 0;
        }
        if ((velocity.x > 0 && this.right) || (velocity.x < 0 && this.left)) {
            velocity.x = 0;
        }

        if (velocity.y < 0) {
            velocity.y = checkDownSpeed(velocity.y);
        }
        else if (velocity.y > 0) {
            velocity.y = checkUpSpeed(velocity.y);
        }
    }

    void Jump() {
        verticalMomentum = jumpForce;
        isGrounded = false;
    }

    bool AnyVoxel(Vector3[] directions) {
        foreach (Vector3 direction in directions) {
            if (world.CheckVoxel(transform.position + direction)) {
                return true;
            }
        }
        return false;
    }

    private float checkDownSpeed(float speed) {
        bool collision = AnyVoxel(new Vector3[] {
            new Vector3(-playerWidth, speed, -playerWidth),
            new Vector3(-playerWidth, speed,  playerWidth),
            new Vector3( playerWidth, speed, -playerWidth),
            new Vector3( playerWidth, speed,  playerWidth),
        });

        if (collision) {
            this.isGrounded = true;
            return 0;
        }
        else {
            this.isGrounded = false;
            return speed;
        }
    }

    private float checkUpSpeed(float speed) {
        bool collision = AnyVoxel(new Vector3[] {
            new Vector3(-playerWidth, 2.0f + speed, -playerWidth),
            new Vector3(-playerWidth, 2.0f + speed,  playerWidth),
            new Vector3( playerWidth, 2.0f + speed, -playerWidth),
            new Vector3( playerWidth, 2.0f + speed,  playerWidth),
        });

        if (collision) {
            return 0;
        }
        else {
            return speed;
        }
    }

    public bool front {
        get {
            return AnyVoxel(new Vector3[] {
                new Vector3(0.0f, 0.0f, playerWidth),
                new Vector3(0.0f, 1.0f, playerWidth)
            });
        }
    }

    public bool back {
        get {
            return AnyVoxel(new Vector3[] {
                new Vector3(0.0f, 0.0f, -playerWidth),
                new Vector3(0.0f, 1.0f, -playerWidth)
            });
        }
    }

    public bool left {
        get {
            return AnyVoxel(new Vector3[] {
                new Vector3(-playerWidth, 0.0f, 0.0f),
                new Vector3(-playerWidth, 1.0f, 0.0f)
            });
        }
    }

    public bool right {
        get {
            return AnyVoxel(new Vector3[] {
                new Vector3(playerWidth, 0.0f, 0.0f),
                new Vector3(playerWidth, 1.0f, 0.0f)
            });
        }
    }
}
