CREATE TABLE messages (
    id SERIAL PRIMARY KEY NOT NULL,
    message_content VARCHAR NOT NULL,
    sent_date TIMESTAMP,
    sender_id UUID NOT NULL,
    recipient_id UUID NOT NULL,
    CONSTRAINT fk_sender FOREIGN KEY (sender_id) REFERENCES profiles(user_id),
    CONSTRAINT fk_recipient FOREIGN KEY (recipient_id) REFERENCES profiles(user_id)
);