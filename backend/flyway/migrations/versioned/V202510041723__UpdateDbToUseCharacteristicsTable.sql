ALTER TABLE profiles
DROP CONSTRAINT IF EXISTS fk_species;

ALTER TABLE profiles
DROP CONSTRAINT IF EXISTS fk_planet;

ALTER TABLE profiles
DROP CONSTRAINT IF EXISTS fk_gender;

     
     
ALTER TABLE profiles
DROP COLUMN IF EXISTS species_id;

ALTER TABLE profiles
DROP COLUMN IF EXISTS planet_id;

ALTER TABLE profiles
DROP COLUMN IF EXISTS gender_id;

ALTER TABLE profiles
DROP COLUMN IF EXISTS height_in_galactic_inches;

ALTER TABLE profiles
DROP COLUMN IF EXISTS galactic_date_of_birth;

     
     
DROP TABLE IF EXISTS planets;
DROP TABLE IF EXISTS species;
DROP TABLE IF EXISTS gender;
