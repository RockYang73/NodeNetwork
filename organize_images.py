import sqlite3
import os
import re
from PIL import Image

DB_PATH = 'ActuatorApp/Assets/Database.db'
BASE_DIR = 'ActuatorApp/Assets/Products'

# Mapping table names to folder names
# Based on Repositories and folders found earlier
# Actuator -> Actuator
# Control -> Control
# Controlbox -> Controlbox
# Touch -> T-touch (Maybe? The folder is T-touch, table is Touch)
# Columns -> Actuator (Often columns are grouped with actuators or have their own? Folder list showed TFL/TL series in Actuator folder in my download script, but maybe they should be elsewhere? The previous file list showed only Actuator, Control, Controlbox, T-touch folders.)

# Let's assume the user wants to organize files within their CURRENT folders, but matching names from the DB.
# We need to know which table corresponds to which folder to validate names correctly.
# Actuator table -> Actuator folder
# Control table -> Control folder
# Controlbox table -> Controlbox folder
# Touch table -> T-touch folder?
# Columns table -> ?? (Maybe Actuator folder too? Lifting columns are often linear actuators.)

TABLE_MAP = {
    'Actuator': 'Actuator',
    'Control': 'Control',
    'Controlbox': 'Controlbox',
    'Touch': 'T-touch',
    'Columns': 'Actuator' # Assuming columns go to Actuator folder based on download script behavior, but let's check DB.
}

def get_db_products():
    products = {
        'Actuator': set(),
        'Control': set(),
        'Controlbox': set(),
        'T-touch': set()
    }
    
    try:
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        # Get tables
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table';")
        tables = [row[0] for row in cursor.fetchall()]
        print(f"Tables in DB: {tables}")

        # Fetch names
        for table, folder in TABLE_MAP.items():
            if table in tables:
                try:
                    # Check if 'Name' column exists (it should based on Entity definitions)
                    cursor.execute(f"SELECT Name FROM {table}")
                    names = [row[0] for row in cursor.fetchall() if row[0]]
                    
                    # Normalize DB names for comparison (strip spaces)
                    for name in names:
                        if folder in products:
                            products[folder].add(name.strip())
                except sqlite3.OperationalError as e:
                    print(f"Error reading table {table}: {e}")
            else:
                print(f"Table {table} not found in DB.")
                
        conn.close()
    except Exception as e:
        print(f"Database error: {e}")
        return None
        
    return products

def normalize_filename(filename):
    # Remove extension
    name = os.path.splitext(filename)[0]
    # Remove common prefixes/suffixes found in the directory listing earlier
    # e.g., PIC_MA3_01 -> MA3
    # e.g., pr_ta1... -> TA1 (already handled by download script, but maybe they should be elsewhere? The previous file list showed only Actuator, Control, Controlbox, T-touch folders.)
    
    # Regex to extract the core model name.
    # Logic: Look for the longest string that matches a DB entry?
    # Or just strip "PIC_", "pr_", "_01", "_02", "Series"
    
    import re
    
    # Remove 'PIC_' prefix
    name = re.sub(r'^PIC_', '', name, flags=re.IGNORECASE)
    # Remove 'pr_' prefix
    name = re.sub(r'^pr_', '', name, flags=re.IGNORECASE)
    
    # Remove suffixes like _01, _02, _s, _s1, etc.
    # Be careful not to remove parts of the model name if it ends in numbers.
    # Usually model names are like TA1, TA16, MA3.
    # Suffixes are usually _XX where XX is digits or 's' + digits.
    
    # Heuristic: If the name contains a DB name, use the DB name.
    return name

def process_directory(folder_name, valid_names):
    dir_path = os.path.join(BASE_DIR, folder_name)
    if not os.path.exists(dir_path):
        print(f"Directory {dir_path} does not exist.")
        return

    print(f"\n--- Processing {folder_name} ---")
    
    # 1. Identify all files
    files = os.listdir(dir_path)
    
    # Group by normalized name or base name to handle duplicates?
    # Better: iterate all files.
    
    for filename in files:
        file_path = os.path.join(dir_path, filename)
        if not os.path.isfile(file_path):
            continue
            
        name, ext = os.path.splitext(filename)
        ext = ext.lower()
        
        # Skip non-image files?
        if ext not in ['.png', '.jpg', '.jpeg', '.bmp', '.webp', '.gif']:
            continue

        # 2. Convert to PNG if needed
        final_path = file_path
        if ext != '.png':
            print(f"Converting {filename} to PNG...")
            try:
                with Image.open(file_path) as img:
                    # Convert to RGB if necessary (e.g. from RGBA if saving to format not supporting it, but PNG supports RGBA)
                    if img.mode == 'CMYK':
                        img = img.convert('RGB')
                    
                    # Just save as PNG
                    new_name = name + ".png"
                    new_path = os.path.join(dir_path, new_name)
                    
                    # Check if target PNG already exists
                    if os.path.exists(new_path):
                        print(f"  {new_name} already exists. Removing original {filename}.")
                        # We assume the existing PNG is preferred or we just overwrite?
                        # "Keep only png format". If png exists, we don't need the jpg.
                        # But the jpg might be the NEW download.
                        # Let's overwrite the PNG with the converted JPG content to ensure we have the latest.
                        img.save(new_path, "PNG")
                    else:
                        img.save(new_path, "PNG")
                        
                os.remove(file_path)
                final_path = new_path
                filename = new_name # Update for next step
                name = os.path.splitext(filename)[0] # Update name
            except Exception as e:
                print(f"  Failed to convert {filename}: {e}")
                continue

        # 3. Validate Name
        # Current name is `name` (without extension)
        
        # Exact match
        if name in valid_names:
            print(f"  {name}.png is valid.")
            continue
            
        # Fuzzy match / Cleaning
        # Try to find a match in valid_names
        normalized = normalize_filename(name)
        
        # Check if normalized matches exactly
        if normalized in valid_names:
            print(f"  Renaming {filename} to {normalized}.png")
            new_path = os.path.join(dir_path, normalized + ".png")
            try:
                if os.path.exists(new_path):
                     # Target exists. 
                     # e.g. renaming "PIC_TA1_01.png" to "TA1.png", but "TA1.png" exists.
                     # We should remove the "bad named" file if the "good named" file exists?
                     # Or overwrite?
                     # Let's assume valid name takes precedence.
                     print(f"  Target {normalized}.png exists. Removing {filename}.")
                     os.remove(final_path)
                else:
                    os.rename(final_path, new_path)
            except Exception as e:
                print(f"  Rename failed: {e}")
            continue
        
        # Strip suffixes like _01, _02 and check again
        # e.g. MA3_01 -> MA3
        base_clean = re.sub(r'_\d+$', '', normalized)
        if base_clean != normalized and base_clean in valid_names:
             print(f"  Renaming {filename} to {base_clean}.png")
             new_path = os.path.join(dir_path, base_clean + ".png")
             try:
                if os.path.exists(new_path):
                     print(f"  Target {base_clean}.png exists. Removing {filename}.")
                     os.remove(final_path)
                else:
                    os.rename(final_path, new_path)
             except Exception as e:
                print(f"  Rename failed: {e}")
             continue

        print(f"  Warning: No match found for {filename}. valid examples: {list(valid_names)[:5]}...")

def main():
    print("Fetching DB products...")
    products = get_db_products()
    
    if products:
        # Process folders
        # Note: 'Columns' items in DB might map to 'Actuator' folder.
        # We should probably combine Actuator and Columns sets for the Actuator folder search.
        
        # Combine Columns into Actuator list for the check
        # Re-fetching to be sure about Columns
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        try:
            cursor.execute("SELECT Name FROM Columns")
            cols = [row[0].strip() for row in cursor.fetchall() if row[0]]
            products['Actuator'].update(cols)
            print(f"Added {len(cols)} columns to Actuator list.")
        except:
            pass
        conn.close()

        for folder, names in products.items():
            if names:
                process_directory(folder, names)
            else:
                print(f"No valid names found for {folder}, skipping.")

if __name__ == "__main__":
    main()