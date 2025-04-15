CREATE TABLE reactions (
    id SERIAL PRIMARY KEY NOT NULL,
    reactor_id UUID NOT NULL,
    target_id UUID NOT NULL,
    is_positive BOOLEAN DEFAULT TRUE NOT NULL,
    CONSTRAINT fk_reactor FOREIGN KEY (reactor_id) REFERENCES profiles(user_id),
    CONSTRAINT fk_target FOREIGN KEY (target_id) REFERENCES profiles(user_id)
);