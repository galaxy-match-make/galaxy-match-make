DROP TABLE IF EXISTS messages CASCADE;
DROP TABLE IF EXISTS reactions CASCADE;
DROP TABLE IF EXISTS user_interests CASCADE;
DROP TABLE IF EXISTS profiles CASCADE;
DROP TABLE IF EXISTS interests CASCADE;
DROP TABLE IF EXISTS genders CASCADE;
DROP TABLE IF EXISTS species CASCADE;
DROP TABLE IF EXISTS planets CASCADE;
DROP TABLE IF EXISTS users CASCADE;

CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid() NOT NULL,
    oauth_id VARCHAR UNIQUE NOT NULL,
    inactive BOOLEAN DEFAULT FALSE NOT NULL
);

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

CREATE TABLE IF NOT EXISTS interests (
    id SERIAL PRIMARY KEY NOT NULL,
    interest_name VARCHAR NOT NULL
);

CREATE TABLE IF NOT EXISTS profiles (
    id SERIAL PRIMARY KEY NOT NULL,
    user_id UUID NOT NULL,
    display_name VARCHAR NOT NULL,
    bio VARCHAR,
    avatar_url VARCHAR,
    species_id INT NOT NULL,
    planet_id INT NOT NULL,
    gender_id INT,
    height_in_galactic_inches FLOAT,
    galactic_date_of_birth INT,
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (species_id) REFERENCES species(id),
    FOREIGN KEY (planet_id) REFERENCES planets(id),
    FOREIGN KEY (gender_id) REFERENCES genders(id),
    CONSTRAINT unique_user_profile UNIQUE (user_id)
);

CREATE TABLE IF NOT EXISTS user_interests (
    id SERIAL PRIMARY KEY NOT NULL,
    interest_id INT NOT NULL,
    user_id UUID NOT NULL,
    FOREIGN KEY (interest_id) REFERENCES interests(id),
    FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT unique_user_interest UNIQUE (user_id, interest_id)
);

CREATE TABLE IF NOT EXISTS reactions (
    id SERIAL PRIMARY KEY NOT NULL,
    reactor_id UUID NOT NULL,
    target_id UUID NOT NULL,
    is_positive BOOLEAN DEFAULT TRUE NOT NULL,
    FOREIGN KEY (reactor_id) REFERENCES users(id),
    FOREIGN KEY (target_id) REFERENCES users(id),
    CONSTRAINT unique_reaction_pair UNIQUE (reactor_id, target_id)
);

CREATE TABLE IF NOT EXISTS messages (
    id SERIAL PRIMARY KEY NOT NULL,
    message_content VARCHAR NOT NULL,
    sent_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    sender_id UUID NOT NULL,
    recipient_id UUID NOT NULL,
    FOREIGN KEY (sender_id) REFERENCES users(id),
    FOREIGN KEY (recipient_id) REFERENCES users(id)
);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_profiles_user_id') THEN
        CREATE INDEX idx_profiles_user_id ON profiles(user_id);
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_user_interests_user_id') THEN
        CREATE INDEX idx_user_interests_user_id ON user_interests(user_id);
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_reactions_reactor_id') THEN
        CREATE INDEX idx_reactions_reactor_id ON reactions(reactor_id);
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_reactions_target_id') THEN
        CREATE INDEX idx_reactions_target_id ON reactions(target_id);
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_messages_sender_id') THEN
        CREATE INDEX idx_messages_sender_id ON messages(sender_id);
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'idx_messages_recipient_id') THEN
        CREATE INDEX idx_messages_recipient_id ON messages(recipient_id);
    END IF;
END $$;
