CREATE TABLE IF NOT EXISTS  profile_preferences (
    id SERIAL PRIMARY KEY NOT NULL,
    profile_id INT NOT NULL,
    characteristic_id INT NOT NULL,
    CONSTRAINT fk_profile_preferences_profiles FOREIGN KEY (profile_id) REFERENCES profiles(id),
    CONSTRAINT fk_profile_preferences_characteristic FOREIGN KEY (characteristic_id) REFERENCES characteristics(id)
);