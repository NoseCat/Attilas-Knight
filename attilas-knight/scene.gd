extends Node2D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	$FieldOrigin/Field.SpaceSize = $FieldOrigin/FieldEdge.position
	$FieldOrigin/Field.Create(15, 10)
	$FieldOrigin/Field.SetRandOnFire(0.25)
	

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
