CREATE TABLE IF NOT EXISTS  profile_attributes (
    id SERIAL PRIMARY KEY NOT NULL,
    profile_id INT NOT NULL,
    characteristic_id INT NOT NULL,
    CONSTRAINT fk_profile_attributes_profiles FOREIGN KEY (profile_id) REFERENCES profiles(id),
    CONSTRAINT fk_profile_attributes_characteristic FOREIGN KEY (characteristic_id) REFERENCES characteristics(id)
);