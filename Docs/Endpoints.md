# Games

- GET /api/games/
  - ?filter=company:ubisoft;name:wolverine&order_by=rating&order=asc/dsc&page=int
- GET /api/games/id

# Users

- GET /api/users

- GET /api/users/id
  - Headers: JWToken, UserId

- GET /api/users/id/games
  - Headers: JWToken, UserId 
  - ?filter=reviewState:Played;platform:ps1&order_by=rating&order=asc/dsc&page=int

- POST /api/users
  - Headers: JWToken, UserId
  - Body: Review fields
  
- PUT /api/users/id
  - Headers: JWToken, UserId
  - Body: Review fields

- DELETE /api/users/id
  - Headers: JWToken, UserId
  - Body: Review Id

# Reviews

- GET /api/reviews

- GET /api/reviews/id
  - Headers: JWToken, UserId

- POST /api/reviews
  - Headers: JWToken, UserId
  - Body: Review fields
  
- PUT /api/reviews/id
  - Headers: JWToken, UserId
  - Body: Review fields

- DELETE /api/reviews/id
  - Headers: JWToken, UserId
  - Body: Review Id

# Developers

- GET /api/developers?page=int

# Platform

- GET /api/platforms?page=int