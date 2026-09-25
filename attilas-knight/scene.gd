extends Node2D

var Horse_pos = Vector2i(0,0)
var King_pos = Vector2i(0,0)

@onready var field = $FieldOrigin/Field
@onready var horse = $FieldOrigin/Horse
@onready var king = $FieldOrigin/King

func _ready() -> void:
	field.SpaceSize = $FieldOrigin/FieldEdge.position
	field.Create(15, 10)
	field.SetRandOnFire(0.25, 1)
	field.TilePressed.connect(_on_tile_pressed)

func _process(delta: float) -> void:
	$State.text = "none"
	if field.legal_count() <= 0:
		$State.text = "stuck"
	if king.grid_pos == horse.grid_pos:
		$State.text = "won"


func _on_generate_pressed() -> void:
	field.SetRandOnFire(float($Chance.text), int($Seed.text))


func _on_rand_seed_pressed() -> void:
	$Seed.text = str(randi() % 10000)


func _on_create_pressed() -> void:
	field.Create(int($X.text), int($Y.text))

func reset_horse():
	horse.set_pos(Horse_pos)
	field.get_tile(Horse_pos).setClear()
	
func reset_king():
	king.set_pos(King_pos)
	field.get_tile(King_pos).setClear()

func _on_horse_rand_pressed() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = int($Seed.text)
	
	var x = rng.randi() % field.GridSize.x
	var y = rng.randi() % field.GridSize.y
	Horse_pos = Vector2i(x,y)
	reset_horse()
	
	x = rng.randi() % field.GridSize.x
	y = rng.randi() % field.GridSize.y
	King_pos = Vector2i(x,y)
	reset_king()
	
func _on_tile_pressed(grid_pos: Vector2i) -> void:
	if horse_manual:
		Horse_pos = grid_pos
		reset_horse()
	
	if king_manual:
		King_pos = grid_pos
		reset_king()
	
var horse_manual = false;
var king_manual = false;

func _on_horse_manual_toggled(toggled_on: bool) -> void:
	horse_manual = toggled_on

func _on_king_manual_toggled(toggled_on: bool) -> void:
	king_manual = toggled_on


func _on_solve_bfs_pressed() -> void:
	if not field.SolveBFS():
		print("BFS: no path")


func _on_solve_dfs_pressed() -> void:
	if not field.SolveDFS():
		print("DFS: no path")


func _on_set_pressed() -> void:
	_on_create_pressed()
	_on_generate_pressed()
	_on_horse_rand_pressed()
