CREATE TABLE IF NOT EXISTS planets (
     id SERIAL PRIMARY KEY NOT NULL,
     planet_name VARCHAR UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS species (
     id SERIAL PRIMARY KEY NOT NULL,
     species_name VARCHAR UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS genders (
         id SERIAL PRIMARY KEY NOT NULL,
         gender VARCHAR NOT NULL
);



ALTER TABLE profiles
ADD COLUMN IF NOT EXISTS species_id  INT NOT NULL;

ALTER TABLE profiles
ADD COLUMN IF NOT EXISTS planet_id INT NOT NULL;

ALTER TABLE profiles
ADD COLUMN IF NOT EXISTS gender_id INT;

ALTER TABLE profiles
ADD COLUMN IF NOT EXISTS height_in_galactic_inches FLOAT;

ALTER TABLE profiles
ADD COLUMN IF NOT EXISTS galactic_date_of_birth INT;



ALTER TABLE profiles
ADD CONSTRAINT IF NOT EXISTS fk_species FOREIGN KEY (species_id) REFERENCES species(id);

ALTER TABLE profiles
ADD CONSTRAINT IF NOT EXISTS fk_planet FOREIGN KEY (planet_id) REFERENCES planets(id);

ALTER TABLE profiles
ADD CONSTRAINT IF NOT EXISTS fk_gender FOREIGN KEY (gender_id) REFERENCES gender(id);

     
     

     
     
