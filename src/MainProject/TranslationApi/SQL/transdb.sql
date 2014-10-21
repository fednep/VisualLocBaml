/*drop database if exists transdb;
create database transdb CHARACTER SET utf8;

use transdb;

drop table if exists rerevisions;
*/

/*
create table revisions (
  id             integer not null auto_increment,
  datecreated    datetime not null,
  primary key (id)
);*/

DROP TABLE IF EXISTS version_info;
CREATE TABLE version_info(
    version         INTEGER NOT NULL
);

INSERT INTO (version_info) VALUES(1);

drop table if exists string_ids;
create table string_ids(
    id              int not null auto_increment,
    filename        varchar(255) not null,
    baml            varchar(255) not null,
    uid             varchar(255) not null,
    category        varchar(255) not null,
    readability     varchar(16) not null,
    localizability  varchar(16) not null,
    description     varchar(16) not null,

    primary key(id),
    index (filename),
    index (baml),
    index (uid),
    index(category),
    index(localizability)
);

drop table if exists original_strings;
create table original_strings (
  id              int not null auto_increment,
  string_id       int not null,
  date_added      date not null,
  revision        int not null,    /* ревизия строки */
  string          text not null,
  is_approved     TINYINT not null default 0,
  is_deleted      TINYINT not null default 0,

  primary key (id),
  index (string_id),
  index (revision),
  index (is_approved)
);

create table translated_strings (
  id              int not null auto_increment,
  string_id       int not null,
  revision        int not null,  /* к какой ревизии из original_strings относится перевод */
  language        varchar(16) not null,
  translation     text not null,

  primary key (id),
  index revision_index (revision),
  index (string_id),
  index (language)
);