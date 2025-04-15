CREATE TABLE profiles (
    id SERIAL PRIMARY KEY NOT NULL,
    user_id UUID NOT NULL UNIQUE,
    display_name VARCHAR NOT NULL,
    bio VARCHAR,
    avatar_url VARCHAR,
    species_id INT NOT NULL,
    planet_id INT NOT NULL,
    gender_id INT,
    height_in_galactic_inches FLOAT,
    galactic_date_of_birth INT,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT fk_species FOREIGN KEY (species_id) REFERENCES species(id),
    CONSTRAINT fk_planet FOREIGN KEY (planet_id) REFERENCES planets(id),
    CONSTRAINT fk_gender FOREIGN KEY (gender_id) REFERENCES genders(id)
);