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

var _gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity")
var _crouching := false

@onready var _collision: CollisionShape3D = $CollisionShape3D
@onready var _body_mesh: MeshInstance3D = $BodyMesh


func _ready() -> void:
	_apply_capsule_height(standing_height if not _crouching else crouch_height)


func _physics_process(delta: float) -> void:
	var want_crouch := Input.is_action_pressed("crouch")
	if want_crouch != _crouching:
		_crouching = want_crouch
		_apply_capsule_height(crouch_height if _crouching else standing_height)

	if not is_on_floor():
		velocity.y -= _gravity * delta

	if Input.is_action_just_pressed("jump") and is_on_floor() and not _crouching:
		velocity.y = JUMP_VELOCITY

	var cam := get_viewport().get_camera_3d()
	var input_vec := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var wish_dir := Vector3.ZERO
	if input_vec.length_squared() > 0.0001:
		if cam:
			var forward := -cam.global_transform.basis.z
			forward.y = 0.0
			var right := cam.global_transform.basis.x
			right.y = 0.0
			if forward.length_squared() > 0.0001 and right.length_squared() > 0.0001:
				forward = forward.normalized()
				right = right.normalized()
				wish_dir = (right * input_vec.x + forward * (-input_vec.y)).normalized()
		else:
			wish_dir = Vector3(input_vec.x, 0.0, -input_vec.y).normalized()

	var target_speed := RUN_SPEED if Input.is_action_pressed("run") and not _crouching else WALK_SPEED
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
