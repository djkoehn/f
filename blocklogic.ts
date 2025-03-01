import { createMachine } from 'xstate';
const nameOfMachine = createMachine({
    /** @xstate-layout N4IgpgJg5mDOIC5QCEA2B7AxgawHQBEBLWTdAOzLEwBdIBiAYVUJwHkyAFQgBzAG0ADAF1EobuliFqhcqJAAPRAEYAHEtwAWDUo0CAbACYArABoQAT0QBaAJwbcqgOxGAvi7NoseIiXKUakLgAkmQAKujoqABGAIYATozMOIIiSCDiktKyaYoIVgDMjgK4RhoGNnoqzmaWeUo2jrgCjmrGbh4YOATEpBRUtBAEcTFQUIRkUIks2ClyGVIyZHK5Vsb5uDYCBvkahaYW1koGxQL5RkpGhq7uIJ5dPr3+A7gcqDGY9ABKhFAAFtRMaazNLzLJLHKHDQqXD5PT5Aytfa1Kz1RrNRHtW6dbw9Pz9QL4YajcaTb5-AFJGbCOYSBbZUArIz5Gy4PR6RwVKpI6zGRp6Gw6fRtG53PAMPEBQZBCCoMB0cLYMBkAASUmBYlpYOWiBUBnUjlh8MRNUh9gESiUjic1w6Xlw4r6ktwoTiPxgcXoCqVnzAspisH41JBmsW2oQKlOJRUzJUu2qBzyBj0RlwBrjNqxdodT0C0tlU2SQY1mVDEIQpXW50t1pNeVKrPNl2Ftq62fxg0JIzGEwLVNSxbp4IZiDZZpUNlje1rKKMLOapyOGdF9olAzoZP+gML-fSIfpCmUDVwk-jyLO6j0uiubhuZHQEDgclFNJL+5WcJTbI5lVP1l2LLTPZMWXB5V0gF9BzDKwjCqVl2U5X88jOYp2SMFpF2A7EV0dAYIK1MsNBsFMqytLlpzOAxcG2dNMLtUCcNzMIImieI8NLYckNjBwBSvYxpyTexLTQjERSw+icw7IluygNi32sfJo1ZIj8kXacjhOc8myXMTcQYwZXnecDg1fIcDzqfJ1k2aipwTFFjAcZtM1bMCpRlMBZNM3JHEvBwjFOTYrmnHyYOOFTHOXNsnRdN0wA9CAPLDIizSUWFSMQ1YLRhIxSh-bSsxcoYuxJBKyzhexsvRVTbL1FktCZDCbyAA */
    id: "Block",
    initial: "Disconnected",
    states: {
        Disconnected: {
            states: {
                InToolbar: {
                    on: {
                        Click: "Dragging"
                    }
                },

                Dragging: {
                    on: {
                        Click: "Placed",
                        RightClick: "InToolbar"
                    }
                },

                Placed: {
                    on: {
                        RightClick: "InToolbar"
                    }
                }
            },

            initial: "InToolbar",

            on: {
                ClickOnPipe: "Connected"
            }
        },

        Connected: {
            states: {
                Idle: {
                    on: {
                        TokenHit: "Triggered",
                        Click: "Dragging"
                    }
                },

                Triggered: {
                    on: {
                        TokenRelease: "Idle"
                    }
                },

                Dragging: {
                    on: {
                        Click: "Idle"
                    }
                }
            },

            initial: "Idle",

            on: {
                RightClick: "Disconnected.InToolbar"
            }
        }
    }
});