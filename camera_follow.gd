extends Camera3D

@export var target_path: NodePath
@export var height := 10.0
@export var distance := 12.0
@export var smooth_speed := 8.0

var _target: Node3D


func _ready() -> void:
	if target_path:
		_target = get_node(target_path) as Node3D


func _physics_process(delta: float) -> void:
	if _target == null:
		return
	var tpos := _target.global_position
	var desired := tpos + Vector3(0.0, height, distance)
	global_position = global_position.lerp(desired, 1.0 - exp(-smooth_speed * delta))
	look_at(tpos, Vector3.UP)
