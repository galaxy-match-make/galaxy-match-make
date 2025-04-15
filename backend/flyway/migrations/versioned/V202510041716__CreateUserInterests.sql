CREATE TABLE user_interests (
    id SERIAL PRIMARY KEY NOT NULL,
    interest_id INT NOT NULL,
    user_id UUID NOT NULL,
    CONSTRAINT fk_interest FOREIGN KEY (interest_id) REFERENCES interests(id),
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES profiles(user_id)
);