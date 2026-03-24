extends CharacterBody3D

const WALK_SPEED := 4.0
const RUN_SPEED := 8.0
const CROUCH_SPEED_MULT := 0.45
const JUMP_VELOCITY := 4.8
const ACCELERATION := 14.0
const FRICTION := 18.0

@export var standing_height := 1.6
@export var crouch_height := 0.85
@export var capsule_radius := 0.35

var _gravity: float
var _crouching := false
var _prev_space_physical := false

@onready var _collision: CollisionShape3D = $CollisionShape3D
@onready var _body_mesh: MeshInstance3D = $BodyMesh


func _ready() -> void:
	_gravity = ProjectSettings.get_setting("physics/3d/default_gravity")
	_apply_capsule_height(standing_height)


func _read_move_input() -> Vector2:
	var ax := Input.get_axis("move_left", "move_right")
	var ay := Input.get_axis("move_forward", "move_back")
	var kx := 0.0
	var ky := 0.0
	if Input.is_physical_key_pressed(KEY_A):
		kx -= 1.0
	if Input.is_physical_key_pressed(KEY_D):
		kx += 1.0
	if Input.is_physical_key_pressed(KEY_W):
		ky -= 1.0
	if Input.is_physical_key_pressed(KEY_S):
		ky += 1.0
	if Input.is_physical_key_pressed(KEY_LEFT):
		kx -= 1.0
	if Input.is_physical_key_pressed(KEY_RIGHT):
		kx += 1.0
	if Input.is_physical_key_pressed(KEY_UP):
		ky -= 1.0
	if Input.is_physical_key_pressed(KEY_DOWN):
		ky += 1.0
	var x := clampf(ax + kx, -1.0, 1.0)
	var y := clampf(ay + ky, -1.0, 1.0)
	var v := Vector2(x, y)
	if v.length_squared() > 1.0:
		v = v.normalized()
	return v


func _resolve_camera_3d() -> Camera3D:
	var p := get_parent()
	if p:
		var c := p.get_node_or_null("Camera3D") as Camera3D
		if c:
			return c
	return get_viewport().get_camera_3d()


func _physics_process(delta: float) -> void:
	var want_crouch := Input.is_action_pressed("crouch") or Input.is_physical_key_pressed(KEY_C)
	if want_crouch != _crouching:
		_crouching = want_crouch
		_apply_capsule_height(crouch_height if _crouching else standing_height)

	if not is_on_floor():
		velocity.y -= _gravity * delta

	var space_now := Input.is_physical_key_pressed(KEY_SPACE)
	var jump_pressed := Input.is_action_just_pressed("jump") or (space_now and not _prev_space_physical)
	_prev_space_physical = space_now

	if jump_pressed and is_on_floor() and not _crouching:
		velocity.y = JUMP_VELOCITY

	var input_vec := _read_move_input()
	var wish_dir := Vector3.ZERO
	if input_vec.length_squared() > 0.0001:
		var cam := _resolve_camera_3d()
		if cam:
			var forward := -cam.global_transform.basis.z
			forward.y = 0.0
			var right := cam.global_transform.basis.x
			right.y = 0.0
			if forward.length_squared() > 0.0001 and right.length_squared() > 0.0001:
				forward = forward.normalized()
				right = right.normalized()
				wish_dir = (right * input_vec.x + forward * (-input_vec.y)).normalized()
		if wish_dir.length_squared() < 0.0001:
			wish_dir = Vector3(input_vec.x, 0.0, -input_vec.y).normalized()

	var run_held := Input.is_action_pressed("run") or Input.is_physical_key_pressed(KEY_SHIFT)
	var target_speed := RUN_SPEED if run_held and not _crouching else WALK_SPEED
	if _crouching:
		target_speed *= CROUCH_SPEED_MULT

	var horiz := Vector3(velocity.x, 0.0, velocity.z)
	if wish_dir.length_squared() > 0.0:
		var target_vel := wish_dir * target_speed
		horiz = horiz.move_toward(target_vel, ACCELERATION * delta)
	else:
		horiz = horiz.move_toward(Vector3.ZERO, FRICTION * delta)

	velocity.x = horiz.x
	velocity.z = horiz.z
	move_and_slide()


func _apply_capsule_height(h: float) -> void:
	var cap_shape := _collision.shape as CapsuleShape3D
	var cap_mesh := _body_mesh.mesh as CapsuleMesh
	cap_shape.radius = capsule_radius
	cap_shape.height = h
	cap_mesh.radius = capsule_radius
	cap_mesh.height = h
	var cy := h * 0.5
	_collision.position.y = cy
	_body_mesh.position.y = cy
