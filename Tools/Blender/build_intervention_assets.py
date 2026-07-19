import bpy
import math
import os
import sys
from pathlib import Path
from mathutils import Vector

args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
project = Path(args[0]).resolve() if args else Path.cwd()
resource_dir = project / "Assets" / "ConcussionVR" / "Resources" / "Blender"
source_dir = project / "ArtSource" / "Blender"
preview_dir = project / "Docs" / "QA" / "Blender"
for directory in (resource_dir, source_dir, preview_dir):
    directory.mkdir(parents=True, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)


def material(name, color, roughness=0.45, metallic=0.0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    return mat


green = material("Canvas Green", (0.08, 0.25, 0.10), 0.78)
dark_green = material("Dark Trim", (0.025, 0.07, 0.035), 0.62)
orange = material("Ball Orange", (0.82, 0.16, 0.025), 0.62)
black = material("Rubber Black", (0.018, 0.022, 0.018), 0.72)
blue = material("Bottle Blue", (0.025, 0.24, 0.58), 0.26, 0.12)
white = material("Bottle Label", (0.72, 0.82, 0.80), 0.55)
brown = material("Bark", (0.16, 0.065, 0.022), 0.9)
needles = material("Pine Needles", (0.025, 0.18, 0.07), 0.82)
needles_light = material("Pine Tips", (0.055, 0.29, 0.12), 0.76)
floor_mat = material("Warm Floor", (0.30, 0.20, 0.11), 0.62)
wall_mat = material("Clinic Wall", (0.43, 0.57, 0.58), 0.8)
teal_mat = material("Clinical Teal", (0.04, 0.42, 0.40), 0.42)


# Fine pebble grain keeps the basketball readable at close range without
# relying on an external texture map.
ball_nodes = orange.node_tree.nodes
ball_links = orange.node_tree.links
ball_bsdf = ball_nodes.get("Principled BSDF")
ball_noise = ball_nodes.new("ShaderNodeTexNoise")
ball_noise.inputs["Scale"].default_value = 82.0
ball_noise.inputs["Detail"].default_value = 2.0
ball_noise.inputs["Roughness"].default_value = 0.68
ball_bump = ball_nodes.new("ShaderNodeBump")
ball_bump.inputs["Strength"].default_value = 0.16
ball_bump.inputs["Distance"].default_value = 0.018
ball_links.new(ball_noise.outputs["Fac"], ball_bump.inputs["Height"])
ball_links.new(ball_bump.outputs["Normal"], ball_bsdf.inputs["Normal"])


def smooth(obj):
    if obj.type == "MESH":
        for polygon in obj.data.polygons:
            polygon.use_smooth = True


def cube(name, location, scale, mat, bevel=0.08):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel > 0:
        modifier = obj.modifiers.new("Edge Softening", "BEVEL")
        modifier.width = bevel
        modifier.segments = 4
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.data.materials.append(mat)
    smooth(obj)
    return obj


def uv_sphere(name, location, scale, mat, segments=48, rings=24):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mat)
    smooth(obj)
    return obj


def cylinder(name, location, radius, depth, mat, vertices=48, bevel=0.04):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=location)
    obj = bpy.context.object
    obj.name = name
    if bevel:
        modifier = obj.modifiers.new("Edge Softening", "BEVEL")
        modifier.width = bevel
        modifier.segments = 3
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.data.materials.append(mat)
    smooth(obj)
    return obj


def torus(name, location, major, minor, mat, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major, minor_radius=minor, major_segments=64, minor_segments=16,
        location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(mat)
    smooth(obj)
    return obj


def curve_strap(name, points, bevel, mat):
    curve = bpy.data.curves.new(name, "CURVE")
    curve.dimensions = "3D"
    curve.bevel_depth = bevel
    curve.bevel_resolution = 4
    spline = curve.splines.new("BEZIER")
    spline.bezier_points.add(len(points) - 1)
    for point, coordinate in zip(spline.bezier_points, points):
        point.co = coordinate
        point.handle_left_type = "AUTO"
        point.handle_right_type = "AUTO"
    obj = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    return obj


def collect_since(before, name):
    objects = [obj for obj in bpy.context.scene.objects if obj.name not in before]
    collection = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(collection)
    for obj in objects:
        for owner in list(obj.users_collection):
            owner.objects.unlink(obj)
        collection.objects.link(obj)
    return collection


def build_backpack():
    before = set(obj.name for obj in bpy.context.scene.objects)
    cube("Backpack_Body", (0, 0, 0), (0.58, 0.30, 0.78), green, 0.18)
    cube("Backpack_Pocket", (0, -0.31, -0.18), (0.43, 0.10, 0.30), dark_green, 0.12)
    curve_strap("Backpack_Handle", [(-0.24, 0, 0.72), (0, 0, 1.02), (0.24, 0, 0.72)], 0.045, dark_green)
    for side in (-1, 1):
        curve_strap(f"Backpack_Strap_{side}", [(0.36 * side, 0.20, 0.55), (0.48 * side, 0.28, -0.15), (0.30 * side, 0.22, -0.68)], 0.055, dark_green)
    return collect_since(before, "Backpack")


def build_ball():
    before = set(obj.name for obj in bpy.context.scene.objects)
    uv_sphere("Ball_Body", (0, 0, 0), (0.58, 0.58, 0.58), orange, 64, 32)
    # Slightly inset, narrow seam cords read as channels rather than hoops.
    seam_radius = 0.574
    seam_width = 0.009
    torus("Ball_Seam_Horizontal", (0, 0, 0), seam_radius, seam_width, black)
    torus("Ball_Seam_Vertical", (0, 0, 0), seam_radius, seam_width, black,
          (0, math.pi / 2, 0))
    torus("Ball_Seam_Curve_Left", (0, 0, 0), seam_radius, seam_width, black,
          (0, math.pi / 2, math.radians(38)))
    torus("Ball_Seam_Curve_Right", (0, 0, 0), seam_radius, seam_width, black,
          (0, math.pi / 2, math.radians(-38)))
    return collect_since(before, "Ball")


def build_bottle():
    before = set(obj.name for obj in bpy.context.scene.objects)
    cylinder("Bottle_Body", (0, 0, 0), 0.30, 1.20, blue, bevel=0.07)
    uv_sphere("Bottle_Shoulder", (0, 0, 0.57), (0.30, 0.30, 0.20), blue)
    cylinder("Bottle_Neck", (0, 0, 0.78), 0.15, 0.32, blue, bevel=0.03)
    cylinder("Bottle_Cap", (0, 0, 0.98), 0.18, 0.16, black, bevel=0.025)
    cylinder("Bottle_Label", (0, 0, -0.05), 0.305, 0.33, white, bevel=0.01)
    return collect_since(before, "Bottle")


def build_pine():
    before = set(obj.name for obj in bpy.context.scene.objects)

    bpy.ops.mesh.primitive_cone_add(vertices=32, radius1=0.18, radius2=0.085, depth=2.4, location=(0, 0, 1.2))
    trunk = bpy.context.object
    trunk.name = "Pine_Trunk"
    trunk.data.materials.append(brown)
    smooth(trunk)

    segments = 16
    rings = [
        (0.42, 0.42), (0.58, 0.72), (0.78, 0.98), (0.98, 1.06),
        (1.18, 1.02), (1.38, 0.96), (1.58, 0.90), (1.78, 0.84),
        (1.98, 0.76), (2.18, 0.68), (2.38, 0.59), (2.58, 0.49),
        (2.78, 0.38), (3.02, 0.22), (3.26, 0.035)
    ]
    vertices = []
    for ring_index, (z, radius) in enumerate(rings):
        phase = (ring_index % 2) * math.pi / segments
        for segment in range(segments):
            angle = segment * math.pi * 2 / segments + phase
            jitter = 1.0 + 0.035 * math.sin(segment * 2.7 + ring_index * 1.9)
            vertices.append((math.cos(angle) * radius * jitter, math.sin(angle) * radius * jitter, z))

    faces = []
    for ring_index in range(len(rings) - 1):
        for segment in range(segments):
            current = ring_index * segments + segment
            following = ring_index * segments + (segment + 1) % segments
            upper = (ring_index + 1) * segments + segment
            upper_following = (ring_index + 1) * segments + (segment + 1) % segments
            faces.append((current, following, upper_following, upper))

    mesh = bpy.data.meshes.new("Pine_Organic_Crown_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    crown = bpy.data.objects.new("Pine_Organic_Crown", mesh)
    bpy.context.collection.objects.link(crown)
    crown.data.materials.append(needles)
    crown.data.materials.append(needles_light)
    for polygon in crown.data.polygons:
        polygon.material_index = (polygon.center.z > 1.65) if hasattr(polygon, "center") else 0
        polygon.use_smooth = True
    return collect_since(before, "Pine")


def export_collection(collection, filename):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in collection.objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = next((obj for obj in collection.objects if obj.type == "MESH"), None)
    bpy.ops.wm.obj_export(
        filepath=str(resource_dir / filename),
        export_selected_objects=True,
        export_materials=True,
        forward_axis="NEGATIVE_Z",
        up_axis="Y")


backpack = build_backpack()
ball = build_ball()
bottle = build_bottle()
pine = build_pine()
for collection, filename in ((backpack, "Backpack.obj"), (ball, "Ball.obj"), (bottle, "Bottle.obj"), (pine, "Pine.obj")):
    export_collection(collection, filename)

for collection, position in ((backpack, (-2.1, 0, 0.9)), (ball, (0, 0, 0.9)), (bottle, (2.1, 0, 0.85))):
    for obj in collection.objects:
        obj.location += Vector(position)
for obj in pine.objects:
    obj.location += Vector((-4.2, 1.4, 0))

cube("Preview_Floor", (0, 0.8, -0.35), (6.5, 5.2, 0.12), floor_mat, 0.02)
cube("Preview_BackWall", (0, 4.5, 2.7), (6.5, 0.12, 3.2), wall_mat, 0.02)
for x in (-3.2, -1.6, 0, 1.6, 3.2):
    cube(f"Preview_Panel_{x}", (x, 4.34, 3.1), (0.58, 0.05, 1.05), teal_mat, 0.05)

bpy.ops.object.light_add(type="AREA", location=(-3, -2, 6))
bpy.context.object.data.energy = 900
bpy.context.object.data.shape = "DISK"
bpy.context.object.data.size = 4.0
bpy.ops.object.light_add(type="AREA", location=(4, 1, 5))
bpy.context.object.data.energy = 700
bpy.context.object.data.color = (0.72, 0.88, 1.0)
bpy.context.object.data.size = 3.0

bpy.ops.object.camera_add(location=(0, -10.5, 2.8), rotation=(math.radians(78), 0, 0))
camera = bpy.context.object
camera.data.lens = 52
bpy.context.scene.camera = camera
direction = Vector((0, 1.0, 1.5)) - camera.location
camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()

scene = bpy.context.scene
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 1280
scene.render.resolution_y = 720
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.filepath = str(preview_dir / "blender-assets-preview.png")
scene.world = bpy.data.worlds.new("Clinical World")
scene.world.color = (0.055, 0.075, 0.08)
scene.render.film_transparent = False
bpy.ops.wm.save_as_mainfile(filepath=str(source_dir / "ConcussionInterventionAssets.blend"))
bpy.ops.render.render(write_still=True)
print(f"BLENDER_PIPELINE_OK {resource_dir}")
