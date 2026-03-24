extends Node3D


func _enter_tree() -> void:
	var window := get_window()
	var screen := DisplayServer.window_get_current_screen()
	window.position = DisplayServer.screen_get_position(screen)
	window.size = DisplayServer.screen_get_size(screen)
