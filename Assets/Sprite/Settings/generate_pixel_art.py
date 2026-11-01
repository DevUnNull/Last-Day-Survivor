"""
Script to generate pixel art 16x16 / 32x32 UI elements for Settings Panel:
- icon_volume.png (16x16)
- icon_bgm.png (16x16)
- icon_sfx.png (16x16)
- icon_vfx.png (16x16)
- icon_close.png (16x16)
- slider_knob.png (16x16)
- toggle_on.png (16x16)
- toggle_off.png (16x16)
- panel_frame_9slice.png (32x32)
"""
from PIL import Image, ImageDraw
import os

OUT_DIR = r"D:\Last-Day-Survivor\Assets\Sprite\Settings"
os.makedirs(OUT_DIR, exist_ok=True)

# 1. icon_volume.png (16x16) - Speaker with sound waves
def make_icon_volume():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pixels = img.load()
    
    # Speaker cone
    gold = (255, 215, 0, 255)
    dark_gold = (180, 140, 20, 255)
    highlight = (255, 245, 140, 255)
    
    # Base box: x=2..4, y=6..10
    for x in range(2, 5):
        for y in range(6, 11):
            pixels[x, y] = dark_gold
    for y in range(7, 10):
        pixels[3, y] = gold
        
    # Cone widening:
    # col 5: y=5..11
    for y in range(5, 12): pixels[5, y] = gold
    # col 6: y=4..12
    for y in range(4, 13): pixels[6, y] = gold
    # col 7: y=3..13
    for y in range(7, 8): pixels[6, y] = highlight
    for y in range(3, 14): pixels[7, y] = dark_gold
    pixels[7, 8] = gold
    
    # Sound waves
    wave_color = (255, 230, 100, 255)
    # Wave 1
    pixels[9, 6] = wave_color
    pixels[10, 7] = wave_color
    pixels[10, 8] = wave_color
    pixels[10, 9] = wave_color
    pixels[9, 10] = wave_color
    
    # Wave 2
    pixels[12, 4] = wave_color
    pixels[13, 5] = wave_color
    pixels[13, 6] = wave_color
    pixels[14, 7] = wave_color
    pixels[14, 8] = wave_color
    pixels[14, 9] = wave_color
    pixels[13, 10] = wave_color
    pixels[13, 11] = wave_color
    pixels[12, 12] = wave_color
    
    img.save(os.path.join(OUT_DIR, "icon_volume.png"))

# 2. icon_bgm.png (16x16) - Music Note (Eighth notes connected)
def make_icon_bgm():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pixels = img.load()
    
    cyan = (0, 230, 255, 255)
    dark_cyan = (0, 140, 180, 255)
    light_cyan = (180, 250, 255, 255)
    
    # Top beam: x=4..12, y=3..4
    for x in range(4, 13):
        pixels[x, 3] = cyan
        pixels[x, 4] = dark_cyan
    pixels[4, 3] = light_cyan
    pixels[12, 3] = light_cyan
    
    # Stems: left x=4, y=5..10; right x=12, y=5..8
    for y in range(5, 11):
        pixels[4, y] = cyan
    for y in range(5, 9):
        pixels[12, y] = cyan
        
    # Note head 1 (left): ellipse at (2..5, 9..12)
    head1 = [(2,10), (3,10), (4,10),
             (1,11), (2,11), (3,11), (4,11),
             (2,12), (3,12)]
    for (x,y) in head1:
        pixels[x,y] = cyan
    pixels[2, 11] = light_cyan
    
    # Note head 2 (right): ellipse at (10..13, 7..10)
    head2 = [(10,8), (11,8), (12,8),
             (9,9), (10,9), (11,9), (12,9),
             (10,10), (11,10)]
    for (x,y) in head2:
        pixels[x,y] = cyan
    pixels[10, 9] = light_cyan
    
    img.save(os.path.join(OUT_DIR, "icon_bgm.png"))

# 3. icon_sfx.png (16x16) - Bell / Sound ding
def make_icon_sfx():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pixels = img.load()
    
    orange = (255, 140, 20, 255)
    gold = (255, 210, 50, 255)
    dark_orange = (160, 70, 0, 255)
    white = (255, 255, 200, 255)
    
    # Bell top handle: (7, 2), (8, 2)
    pixels[7, 2] = gold
    pixels[8, 2] = gold
    
    # Bell curve:
    # y=3..4: width 4 (6..9)
    for x in range(6, 10): pixels[x, 3] = gold; pixels[x, 4] = gold
    # y=5..7: width 6 (5..10)
    for y in range(5, 8):
        for x in range(5, 11): pixels[x, y] = gold
    # y=8..10: width 8 (4..11)
    for y in range(8, 11):
        for x in range(4, 12): pixels[x, y] = gold
    # Rim at y=11: (3..12)
    for x in range(3, 13): pixels[x, 11] = dark_orange
    for x in range(4, 12): pixels[x, 11] = orange
    
    # Clapper at y=12, 13 (7, 8)
    pixels[7, 12] = dark_orange; pixels[8, 12] = dark_orange
    pixels[7, 13] = gold; pixels[8, 13] = gold
    
    # Highlight
    pixels[6, 5] = white
    pixels[6, 6] = white
    pixels[5, 8] = white
    
    img.save(os.path.join(OUT_DIR, "icon_sfx.png"))

# 4. icon_vfx.png (16x16) - Magical sparkles / 4-point star & burst
def make_icon_vfx():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pixels = img.load()
    
    purple = (200, 80, 255, 255)
    magenta = (255, 100, 220, 255)
    core = (255, 255, 255, 255)
    glow = (140, 40, 200, 255)
    
    # Main star center at (8, 7)
    cx, cy = 8, 7
    # Long spikes
    for d in range(1, 6):
        pixels[cx, cy - d] = magenta if d < 4 else glow
        pixels[cx, cy + d] = magenta if d < 4 else glow
        pixels[cx - d, cy] = magenta if d < 4 else glow
        pixels[cx + d, cy] = magenta if d < 4 else glow
        
    # Core
    for dx in [-1, 0, 1]:
        for dy in [-1, 0, 1]:
            pixels[cx + dx, cy + dy] = purple
    pixels[cx, cy] = core
    pixels[cx-1, cy] = core
    pixels[cx, cy-1] = core
    
    # Small sparkle top-left (2, 2)
    pixels[2, 2] = core
    pixels[2, 1] = purple; pixels[2, 3] = purple
    pixels[1, 2] = purple; pixels[3, 2] = purple
    
    # Small sparkle bottom-right (13, 12)
    pixels[13, 12] = core
    pixels[13, 11] = purple; pixels[13, 13] = purple
    pixels[12, 12] = purple; pixels[14, 12] = purple
    
    img.save(os.path.join(OUT_DIR, "icon_vfx.png"))

# 5. icon_close.png (16x16) - Pixel Red 'X' with border
def make_icon_close():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pixels = img.load()
    
    red = (235, 60, 60, 255)
    dark_red = (120, 20, 20, 255)
    light_red = (255, 140, 140, 255)
    
    # 2-pixel thick diagonal 'X' from (3,3) to (12,12)
    for i in range(3, 13):
        # Top-left to bottom-right
        pixels[i, i] = red
        if i + 1 < 13: pixels[i + 1, i] = dark_red
        # Top-right to bottom-left
        pixels[15 - i, i] = red
        if 15 - i - 1 >= 3: pixels[15 - i - 1, i] = dark_red
        
    # Highlights on core cross
    pixels[7, 7] = light_red
    pixels[8, 8] = light_red
    pixels[7, 8] = light_red
    pixels[8, 7] = light_red
    
    img.save(os.path.join(OUT_DIR, "icon_close.png"))

# 6. slider_knob.png (16x16) - Golden round gem / button
def make_slider_knob():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pixels = img.load()
    
    gold = (255, 200, 40, 255)
    dark_gold = (160, 100, 10, 255)
    white = (255, 255, 220, 255)
    outline = (40, 20, 5, 255)
    
    # Circle diameter 12 (center 7.5, 7.5 -> coords 2..13)
    cx, cy = 7.5, 7.5
    for x in range(16):
        for y in range(16):
            d2 = (x - cx)**2 + (y - cy)**2
            if d2 <= 5.8**2:
                if d2 > 4.8**2:
                    pixels[x, y] = outline
                elif d2 > 3.5**2:
                    pixels[x, y] = dark_gold
                else:
                    pixels[x, y] = gold
                    
    # Specular shine
    pixels[6, 5] = white
    pixels[7, 5] = white
    pixels[6, 6] = white
    
    img.save(os.path.join(OUT_DIR, "slider_knob.png"))

# 7. toggle_on.png (24x14) - Pill switch ON
def make_toggle_on():
    img = Image.new("RGBA", (24, 14), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Background capsule (greenish teal)
    draw.rounded_rectangle([0, 1, 23, 12], radius=6, fill=(35, 160, 110, 255), outline=(15, 60, 40, 255))
    # Inner glowing track
    draw.rounded_rectangle([2, 3, 21, 10], radius=4, fill=(50, 205, 140, 255))
    # Knob on the right (x=11..21)
    draw.ellipse([11, 2, 21, 12], fill=(255, 255, 255, 255), outline=(15, 60, 40, 255))
    img.save(os.path.join(OUT_DIR, "toggle_on.png"))

# 8. toggle_off.png (24x14) - Pill switch OFF
def make_toggle_off():
    img = Image.new("RGBA", (24, 14), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Background capsule (dark muted grey)
    draw.rounded_rectangle([0, 1, 23, 12], radius=6, fill=(55, 55, 65, 255), outline=(20, 20, 25, 255))
    # Inner track
    draw.rounded_rectangle([2, 3, 21, 10], radius=4, fill=(35, 35, 45, 255))
    # Knob on the left (x=2..12)
    draw.ellipse([2, 2, 12, 12], fill=(160, 160, 175, 255), outline=(20, 20, 25, 255))
    img.save(os.path.join(OUT_DIR, "toggle_off.png"))

# 9. panel_frame_9slice.png (32x32) - Fantasy Pixel Art Beveled Frame
def make_panel_frame():
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    pixels = img.load()
    
    border_dark = (20, 16, 28, 255)
    border_gold = (195, 145, 45, 255)
    border_gold_light = (255, 220, 110, 255)
    border_gold_shadow = (115, 75, 20, 255)
    inner_bg = (28, 24, 40, 240)
    
    # Fill background
    for x in range(32):
        for y in range(32):
            pixels[x, y] = inner_bg
            
    # Outer dark outline (1px)
    for i in range(32):
        pixels[i, 0] = border_dark
        pixels[i, 31] = border_dark
        pixels[0, i] = border_dark
        pixels[31, i] = border_dark
        
    # Bevel 1 (gold highlight top/left, shadow bottom/right)
    for i in range(1, 31):
        pixels[i, 1] = border_gold_light
        pixels[1, i] = border_gold_light
        pixels[i, 30] = border_gold_shadow
        pixels[30, i] = border_gold_shadow
        
    # Bevel 2 (gold core)
    for i in range(2, 30):
        pixels[i, 2] = border_gold
        pixels[2, i] = border_gold
        pixels[i, 29] = border_gold
        pixels[29, i] = border_gold
        
    # Bevel 3 (inner dark shadow)
    for i in range(3, 29):
        pixels[i, 3] = border_dark
        pixels[3, i] = border_dark
        pixels[i, 28] = border_dark
        pixels[28, i] = border_dark
        
    # Corner ornaments
    for c in [(2,2), (29,2), (2,29), (29,29)]:
        pixels[c[0], c[1]] = border_gold_light
        
    img.save(os.path.join(OUT_DIR, "panel_frame_9slice.png"))

# 10. slider_track.png (16x16 9-sliceable track)
def make_slider_track():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pixels = img.load()
    dark = (15, 12, 22, 255)
    border = (70, 60, 90, 255)
    fill = (32, 26, 44, 255)
    
    # 6px height track in center y=5..10
    for x in range(16):
        for y in range(5, 11):
            if y == 5 or y == 10 or x == 0 or x == 15:
                pixels[x, y] = border
            elif y == 6:
                pixels[x, y] = dark
            else:
                pixels[x, y] = fill
    img.save(os.path.join(OUT_DIR, "slider_track.png"))

# 11. slider_fill.png (16x16 9-sliceable glowing fill)
def make_slider_fill():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    pixels = img.load()
    border = (200, 150, 30, 255)
    light = (255, 230, 100, 255)
    core = (230, 170, 30, 255)
    
    for x in range(16):
        for y in range(5, 11):
            if y == 5 or y == 10 or x == 0 or x == 15:
                pixels[x, y] = border
            elif y == 6:
                pixels[x, y] = light
            else:
                pixels[x, y] = core
    img.save(os.path.join(OUT_DIR, "slider_fill.png"))

if __name__ == "__main__":
    make_icon_volume()
    make_icon_bgm()
    make_icon_sfx()
    make_icon_vfx()
    make_icon_close()
    make_slider_knob()
    make_toggle_on()
    make_toggle_off()
    make_panel_frame()
    make_slider_track()
    make_slider_fill()
    print("All pixel art sprites generated successfully!")

