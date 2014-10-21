/*drop database if exists transdb;
create database transdb CHARACTER SET utf8;

use transdb;

---------------- */

DROP TABLE IF EXISTS version_info;
CREATE TABLE version_info(
    version         INTEGER NOT NULL
);

INSERT INTO version_info (version) VALUES(1);

DROP TABLE IF EXISTS cultures;
CREATE TABLE cultures
(
    culture_code VARCHAR(64) PRIMARY KEY NOT NULL collate nocase
);

DROP TABLE IF EXISTS settings;
CREATE TABLE settings
(
    key            VARCHAR(64) PRIMARY KEY NOT NULL,
    value          BLOB
);

DROP TABLE IF EXISTS assemblies;
CREATE TABLE assemblies
(
    assembly_file VARCHAR(255) PRIMARY KEY NOT NULL collate nocase,
    default_culture VARCHAR(32) NOT NULL collate nocase,
    default_resource VARCHAR(32) NOT NULL collate nocase
);

DROP TABLE IF EXISTS string_ids;
CREATE TABLE string_ids(
    id              INTEGER PRIMARY KEY autoincrement,
    filename        VARCHAR(255) NOT NULL collate nocase,
    baml            VARCHAR(255) NOT NULL collate nocase,
    uid             VARCHAR(255) NOT NULL collate nocase,
    category        VARCHAR(255) NOT NULL,
    readability     VARCHAR(16) NOT NULL,
    localizability  VARCHAR(16) NOT NULL,
    description     VARCHAR(16) NOT NULL,
    max_revision    int not null default 0,
    is_deleted      TINYINT NOT NULL DEFAULT 0,

    FOREIGN KEY (filename) REFERENCES assemblies(assembly_file) ON DELETE CASCADE
);

CREATE INDEX idx_string_ids_filename ON string_ids(filename collate nocase);
CREATE INDEX idx_string_ids_baml ON string_ids(baml collate nocase);
CREATE INDEX idx_string_ids_uid ON string_ids(uid collate nocase);
CREATE INDEX idx_string_ids_category ON string_ids(category);
CREATE INDEX idx_string_ids_localizability ON string_ids(localizability);
CREATE INDEX idx_string_ids_is_deleted ON string_ids(is_deleted);
CREATE INDEX idx_string_ids_max_revision ON string_ids(max_revision);

/* ---------------- */

DROP TABLE IF EXISTS original_strings;
CREATE TABLE original_strings (
  id              INTEGER PRIMARY KEY AUTOINCREMENT,
  string_id       INTEGER NOT NULL,
  date_added      DATE NOT NULL,
  revision        INTEGER NOT NULL,    /* ревизия строки */
  string          TEXT NOT NULL,
  is_approved     TINYINT NOT NULL DEFAULT 0,
  is_deleted      TINYINT NOT NULL DEFAULT 0,

  FOREIGN KEY (string_id) REFERENCES strings_ids(id) ON DELETE CASCADE
);

CREATE INDEX idx_original_strings_string_id ON original_strings(string_id);
CREATE INDEX idx_original_strings_revision ON original_strings(revision);
CREATE INDEX idx_original_strings_is_approved ON original_strings(is_approved);
CREATE INDEX idx_original_strings_is_deleted ON original_strings(is_deleted);

/* ---------------- */
                                
DROP TABLE IF EXISTS translated_strings;
CREATE TABLE translated_strings (
  id              INTEGER PRIMARY KEY AUTOINCREMENT,
  string_id       INTEGER NOT NULL,
  revision        INTEGER NOT NULL,  /* к какой ревизии из original_strings относится перевод */
  culture_code    VARCHAR(16) NOT NULL collate nocase,
  translation     TEXT NOT NULL,

  FOREIGN KEY (string_id) REFERENCES strings_ids(id) ON DELETE CASCADE
  FOREIGN KEY (culture_code) REFERENCES cultures(culture_code collate nocase) ON DELETE CASCADE
);

CREATE INDEX translated_strings_revision ON translated_strings(revision);
