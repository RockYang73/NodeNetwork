from PIL import Image, ImageDraw, ImageFont
import os

source_img_path = r"SpecResource/Spec-SampleCaptured.png"
output_dir = r"Documentation/UserGuide"

def create_guides():
    if not os.path.exists(source_img_path):
        print(f"Source image not found at {source_img_path}")
        return

    try:
        base_img = Image.open(source_img_path).convert("RGBA")
        width, height = base_img.size
        print(f"Image loaded: {width}x{height}")
    except Exception as e:
        print(f"Failed to load image: {e}")
        return

    # Helper to get a font
    try:
        # Try standard Windows font
        font = ImageFont.truetype("arial.ttf", 24)
        font_bold = ImageFont.truetype("arialbd.ttf", 28)
    except:
        font = ImageFont.load_default()
        font_bold = ImageFont.load_default()

    # 1. Selection (Rectangle)
    # Coordinates guess: Start at (400, 200) drag to (800, 600) - covering TBB3, TC16
    img1 = base_img.copy()
    overlay1 = Image.new("RGBA", base_img.size, (0,0,0,0))
    draw1 = ImageDraw.Draw(overlay1)
    sel_rect = [450, 250, 950, 650] # Covering TBB3(Green) and part of TC16(Blue)
    draw1.rectangle(sel_rect, fill=(0, 120, 215, 60), outline=(0, 120, 215, 255), width=3)
    
    # Add Label
    draw1.rectangle([450, 220, 650, 250], fill=(0, 120, 215, 255))
    draw1.text((460, 225), "1. Left Click + Drag", font=font_bold, fill="white")
    
    # Cursor
    # draw1.polygon([(950, 650), (950, 670), (960, 660)], fill="white", outline="black") 
    
    out1 = Image.alpha_composite(img1, overlay1)
    out1.save(os.path.join(output_dir, "1_Selection.png"))
    print("Generated 1_Selection.png")

    # 2. Pan (Hand/Arrows)
    img2 = base_img.copy()
    draw2 = ImageDraw.Draw(img2)
    center_x, center_y = 600, 600 # Empty area bottom middle
    
    # Draw cross arrows
    arrow_len = 40
    draw2.line([(center_x - arrow_len, center_y), (center_x + arrow_len, center_y)], fill="red", width=5)
    draw2.line([(center_x, center_y - arrow_len), (center_x, center_y + arrow_len)], fill="red", width=5)
    # Arrow heads
    # ... simplifying drawing
    
    draw2.text((center_x - 80, center_y - 60), "2. Right Click + Drag", font=font_bold, fill="red")
    draw2.text((center_x - 60, center_y - 30), "(Pan Canvas)", font=font, fill="red")
    
    img2.save(os.path.join(output_dir, "2_Pan.png"))
    print("Generated 2_Pan.png")

    # 3. Cut (Dashed Line)
    img3 = base_img.copy()
    draw3 = ImageDraw.Draw(img3)
    
    # Connection between TC16 (Blue) and TA6 (Orange, top right)
    # Start roughly (1000, 350)
    # Cut line across it
    start_cut = (1050, 300)
    end_cut = (1050, 450)
    
    draw3.line([start_cut, end_cut], fill="red", width=4)
    # Simulate dashes manually if needed, but solid red is clear enough for a guide
    
    draw3.text((1060, 370), "3. Middle Click + Drag", font=font_bold, fill="red")
    draw3.text((1060, 400), "(Cut Connection)", font=font, fill="red")
    
    img3.save(os.path.join(output_dir, "3_Cut.png"))
    print("Generated 3_Cut.png")

    # 4. Enter Group (Double Click)
    img4 = base_img.copy()
    draw4 = ImageDraw.Draw(img4)
    
    # Target TC16 (Blue) center roughly (900, 350)
    target_x, target_y = 900, 350
    radius = 40
    
    draw4.ellipse([target_x-radius, target_y-radius, target_x+radius, target_y+radius], outline="lime", width=5)
    draw4.text((target_x + 50, target_y - 10), "4. Double Left Click", font=font_bold, fill="lime")
    draw4.text((target_x + 50, target_y + 20), "(Enter Group)", font=font, fill="lime")
    
    img4.save(os.path.join(output_dir, "4_EnterGroup.png"))
    print("Generated 4_EnterGroup.png")

    # 5. Back Button (Mockup)
    img5 = base_img.copy()
    draw5 = ImageDraw.Draw(img5)
    
    # Draw a button at top left of canvas (right of tool panel)
    # Panel ends around x=280. Top bar ends y=40.
    # Button at (300, 50)
    btn_rect = [300, 50, 400, 90]
    draw5.rectangle(btn_rect, fill="lightgray", outline="gray", width=2)
    draw5.text((320, 58), "Back", font=font, fill="black")
    
    # Highlight it
    draw5.ellipse([290, 40, 410, 100], outline="magenta", width=4)
    draw5.text((420, 60), "5. Click Back", font=font_bold, fill="magenta")
    draw5.text((420, 90), "(Return to Parent)", font=font, fill="magenta")
    
    img5.save(os.path.join(output_dir, "5_BackButton.png"))
    print("Generated 5_BackButton.png")

    # 6. Collapse/Expand (Arrow)
    img6 = base_img.copy()
    draw6 = ImageDraw.Draw(img6)
    
    # Target TA6 (Orange Top Right) header arrow
    # TA6 is roughly at x=1250, y=250
    # Arrow is top right of node.
    arrow_x, arrow_y = 1350, 230 # Approximation
    
    draw6.ellipse([arrow_x-20, arrow_y-20, arrow_x+20, arrow_y+20], outline="cyan", width=4)
    draw6.line([arrow_x+30, arrow_y, arrow_x+80, arrow_y], fill="cyan", width=2)
    draw6.text((arrow_x + 90, arrow_y - 10), "6. Left Click", font=font_bold, fill="cyan")
    draw6.text((arrow_x + 90, arrow_y + 20), "(Expand/Collapse)", font=font, fill="cyan")
    
    img6.save(os.path.join(output_dir, "6_ExpandCollapse.png"))
    print("Generated 6_ExpandCollapse.png")

if __name__ == "__main__":
    create_guides()
