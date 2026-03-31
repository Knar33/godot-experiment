extends AnimationTree
## Bridge for C#: Godot's GDScript Object.set() for parameters/run/blend_amount etc. matches the
## inspector; some C# Variant paths fail to update the internal property map reliably.

func _ready() -> void:
	# GH-105275: force float storage on blend params (0.0 not int 0).
	var blend_paths: Array[String] = [
		"parameters/run/blend_amount",
		"parameters/speed/blend_amount",
		"parameters/state/blend_amount",
		"parameters/air_dir/blend_amount",
		"parameters/gun/blend_amount",
	]
	for p in blend_paths:
		set(p, 0.0)
	set("parameters/scale/scale", 1.0)


func set_blend_param(path: String, value: float) -> void:
	set(path, value)
