import React, { Component } from 'react';
import NaviBar from './Components/NavBar';
import axios from 'axios';
import './GameAdvert.css';
import Footer from './Components/Footer';
import { Storage } from 'aws-amplify';
import GameAdSlides from './Components/GameAdSlides';
import { Grid, Row, Col, Glyphicon, Button, Form, FormControl, FormGroup, ControlLabel, Jumbotron, Image } from 'react-bootstrap';
import './BodyWrap.css'

class GameAdvert extends Component {

    constructor(props) {
        super(props);

        this.state = {
            status: 0,
            gamedata: [],
            numImages: 0,
            gameImage0: "",
            gameImage1: "",
            gameImage2: "",
            gameImage3: "",
            gameImage4: "",
            file: ""
        };
    }



    componentDidMount() {

        const urlParams = new URLSearchParams(window.location.search);
        const urlGameid = urlParams.get('gameId');
        const getRequest = `api/v1/Public/rds/games/gamesbyid?gameid=` + urlGameid;
        try {
            axios.get(getRequest)
                .then(res => {
                    const gamedata = res.data;
                    this.setState({ gamedata: gamedata });
                    this.setState({ status: 2 });
                    this.setState({ numImages: gamedata.gameImg.length });
                    if (res.status === 500) {
                        this.setState({ status: 1 });
                    }
                    if (res.status != 200) {
                        this.setState({ status: 1 });
                    }
                })
                .then(async (gamedata) => {
                    this.setState({ gameImage0: await Storage.get(this.state.gamedata.gameImg[0], { level: 'public' }) })
                    this.setState({ gameImage1: await Storage.get(this.state.gamedata.gameImg[1], { level: 'public' }) })
                    this.setState({ gameImage2: await Storage.get(this.state.gamedata.gameImg[2], { level: 'public' }) })
                    this.setState({ gameImage3: await Storage.get(this.state.gamedata.gameImg[3], { level: 'public' }) })
                    this.setState({ gameImage4: await Storage.get(this.state.gamedata.gameImg[4], { level: 'public' }) })
                    this.setState({ file: await Storage.get(this.state.gamedata.gamePath, { level: 'public' }) })
                })
        }
        catch{
            this.setState({ status: 1 });
        }

    }



    render(props) {
        if (this.state.gamedata.gameStatus === 'a' || this.state.status === 2) {
            const creatorLink = "/games?search=" + this.state.gamedata.gameCreatorName;
            const genres = [];
            var glyph;
            var slideshow;
            var downloadButton;
            const options = { year: 'numeric', month: 'long', day: 'numeric' };
            const date = new Date(this.state.gamedata.gameSubmissionDateUtc);

            if (this.state.gamedata.gameAvailableToDownload) {
                downloadButton = (
                    <a href={this.state.file} download>
                        <Button bsStyle='info'>Download Game</Button>
                    </a>
                )
            }

            if (this.state.gamedata.gameGenreAction === true) {
                genres.push("Action");
            }
            if (this.state.gamedata.gameGenreAdventure === true) {
                genres.push("Adventure");
            }
            if (this.state.gamedata.gameGenreFighting === true) {
                genres.push("Fighting");
            }
            if (this.state.gamedata.gameGenrePlatformer === true) {
                genres.push("Platformer");
            }
            if (this.state.gamedata.gameGenrePuzzle === true) {
                genres.push("Puzzle");
            }
            if (this.state.gamedata.gameGenreRPG === true) {
                genres.push("RPG");
            }
            if (this.state.gamedata.gameGenreRacing === true) {
                genres.push("Racing");
            }
            if (this.state.gamedata.gameGenreRhythm === true) {
                genres.push("Rhythm");
            }
            if (this.state.gamedata.gameGenreShooter === true) {
                genres.push("Shooter");
            }
            if (this.state.gamedata.gameGenreSports === true) {
                genres.push("Sports");
            }
            if (this.state.gamedata.gameGenreStrategy === true) {
                genres.push("Strategy");
            }
            if (this.state.gamedata.gameGenreSurvival === true) {
                genres.push("Survival");
            }

            const genreList = genres.join(", ");

            if (this.state.gamedata.gameOnArcade === true) {
                glyph = <Glyphicon className='Check' glyph='ok'></Glyphicon>;
            }
            else {
                glyph = <Glyphicon className='Nocheck' glyph='remove'></Glyphicon>;
            }

            switch (this.state.numImages) {
                case 1:
                    slideshow = <Image responsive width={896} height={504} src={this.state.gameImage0} />;
                    break;
                case 2:
                    slideshow = <GameAdSlides numImages={this.state.numImages}
                        numItems={2}
                        gameSlide0={this.state.gameImage0}
                        gameSlide1={this.state.gameImage1} />
                    break;
                case 3:
                    slideshow = <GameAdSlides numImages={this.state.numImages}
                        numItems={3}
                        gameSlide0={this.state.gameImage0}
                        gameSlide1={this.state.gameImage1}
                        gameSlide2={this.state.gameImage2} />
                    break;
                case 4:
                    slideshow = <GameAdSlides numImages={this.state.numImages}
                        numItems={4}
                        gameSlide0={this.state.gameImage0}
                        gameSlide1={this.state.gameImage1}
                        gameSlide2={this.state.gameImage2}
                        gameSlide3={this.state.gameImage3} />
                    break;
                case 5:
                    slideshow = <GameAdSlides numImages={this.state.numImages}
                        numItems={5}
                        gameSlide0={this.state.gameImage0}
                        gameSlide1={this.state.gameImage1}
                        gameSlide2={this.state.gameImage2}
                        gameSlide3={this.state.gameImage3}
                        gameSlide4={this.state.gameImage4} />
                    break;
            }


            return (
                <div className='FullPage'>
                    <div className='BodyWrap'>
                        <NaviBar />
                        <div className='GameAdDiv'>
                            <Jumbotron style={{ marginBottom: 10, backgroundColor: '#272727' }}>
                                <Grid fluid>
                                    <Row style={{ marginLeft: 0, marginRight: 0 }}>
                                        <Col md={6} mdOffset={3} style={{ paddingLeft: 0, paddingRight: 0 }}>
                                            <h1 className="GameTitle">{this.state.gamedata.gameName}</h1>
                                        </Col>
                                    </Row>
                                </Grid>
                            </Jumbotron>
                            <Grid fluid style={{ paddingLeft: 0, paddingRight: 0 }}>
                                <Row style={{ marginLeft: 0, marginRight: 0 }}>
                                    <Col md={6} mdOffset={2} sm={6} smOffset={2} style={{ paddingLeft: 0, paddingRight: 0 }}>
                                        {slideshow}
                                    </Col>
                                    <Col md={2} mdOffset={1} sm={2} smOffset={1} style={{ paddingLeft: 0, paddingRight: 0 }}>
                                        <Form>
                                            <FormGroup>
                                                <ControlLabel>Creator</ControlLabel>
                                                <a href={creatorLink}><FormControl.Static>{this.state.gamedata.gameCreatorName}</FormControl.Static></a>
                                            </FormGroup>
                                            <FormGroup>
                                                <ControlLabel>Date Published</ControlLabel>
                                                <FormControl.Static>{date.toLocaleDateString("en-US", options)}</FormControl.Static>
                                            </FormGroup>
                                            <FormGroup>
                                                <ControlLabel>Genres</ControlLabel>
                                                <FormControl.Static>{genreList}</FormControl.Static>
                                            </FormGroup>
                                            <FormGroup>
                                                <ControlLabel>Available On Arcade Machines</ControlLabel>
                                                {glyph}
                                            </FormGroup>
                                            {downloadButton}
                                        </Form>
                                    </Col>
                                </Row>
                                <Row style={{ marginLeft: 0, marginRight: 0 }}>
                                    <Col md={5} mdOffset={2} style={{ paddingLeft: 0, paddingRight: 0 }}>
                                        <h3>About the game</h3>
                                        <p>{this.state.gamedata.gameDescription}</p>
                                    </Col>
                                    <Col md={2} mdOffset={1} style={{ paddingLeft: 0, paddingRight: 0 }}>
                                        <h3>Controls</h3>
                                        <p>{this.state.gamedata.gameControls}</p>
                                    </Col>
                                </Row>
                            </Grid>
                        </div>
                    </div>
                    <Footer />
                </div>
            )
        }
        else if (this.state.status === 1 || this.state.gamedata != 'a') {
            return (
                <div className='FullPage'>
                    <div className='BodyWrap'>
                        <NaviBar />
                        <div className="FourOFourSpace">
                            <h2>404: Game Not Found</h2>
                        </div>
                    </div>
                    <Footer />
                </div>

            )
        }
        else {
            return (
                <div className='FullPage'>
                    <div className='BodyWrap'>
                        <NaviBar />
                        <div className="WhiteSpace">
                        </div>
                    </div>
                    <Footer />
                </div>
            )
        }
    }
}

export default GameAdvert;