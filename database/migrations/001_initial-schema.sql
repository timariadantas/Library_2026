
-- Migration: 001 - Initial Schema

-- Enums
CREATE TYPE book_condition AS ENUM (
    'perfect',
    'good',
    'bad',
    'useless',
    'disable'
);

CREATE TYPE book_cover AS ENUM (
    'paper',
    'hardcover'
);

CREATE TYPE book_genre AS ENUM (
    'adventure',
    'romance',
    'fantasy',
    'sci_fi',
    'history',
    'horror',
    'distopian',
    'biography',
    'self_help',
    'memory',
    'true_crime',
    'poetry',
    'graphic_novel'
);

-- Users
CREATE TABLE users (
    id CHAR(26) PRIMARY KEY,

    name VARCHAR(256) NOT NULL,

    email VARCHAR(512) NOT NULL,

    created_at TIMESTAMP NOT NULL DEFAULT NOW(),

    inactived_at TIMESTAMP,

    active BOOLEAN NOT NULL DEFAULT TRUE
);

-- Books
CREATE TABLE books (
    isbn CHAR(13) PRIMARY KEY,

    title VARCHAR(512) NOT NULL,

    release_year INT NOT NULL,

    summary TEXT,

    author TEXT NOT NULL,

    page_len INT,

    publisher TEXT
);

-- Genre
CREATE TABLE genre (
    book_id CHAR(13) NOT NULL,

    genre book_genre NOT NULL,

    PRIMARY KEY (book_id, genre),

    CONSTRAINT fk_genre_book
        FOREIGN KEY (book_id, genre),
        REFERENCES books(isbn)
);

-- Portfolio
CREATE TABLE portfolio (
    id CHAR(26) PRIMARY KEY,

    book_id CHAR(13) NOT NULL,

    condition book_condition NOT NULL,

    cover book_cover NOT NULL,

    CONSTRAINT fk_portfolio_book
        FOREIGN KEY (book_id)
        REFERENCES books(isbn)
);

-- Loan
CREATE TABLE loan (
    id CHAR(26) PRIMARY KEY,

    portfolio_id CHAR(26) NOT NULL,

    user_id CHAR(26) NOT NULL,

    start_at TIMESTAMP NOT NULL DEFAULT NOW(),

    return_at TIMESTAMP,

    period INT NOT NULL DEFAULT 30,

    loan_condition book_condition,

    return_condition book_condition,

    CONSTRAINT fk_loan_portfolio
        FOREIGN KEY (portfolio_id)
        REFERENCES portfolio(id),

    CONSTRAINT fk_loan_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
);

CREATE INDEX idx_genre_book_id ON genre(book_id);
CREATE INDEX idx_portfolio_book_id ON portfolio(book_id);
CREATE INDEX idx_loan_user_id ON loan(user_id);

